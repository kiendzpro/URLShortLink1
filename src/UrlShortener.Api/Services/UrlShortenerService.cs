using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Api.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly IUrlRepository _urlRepository;
        private readonly ICodeGenerator _codeGenerator;
        private readonly IUrlCacheService _cacheService;
        private readonly UrlShortenerSettings _settings;
        private readonly ILogger<UrlShortenerService> _logger;

        public UrlShortenerService(
            IUrlRepository urlRepository,
            ICodeGenerator codeGenerator,
            IUrlCacheService cacheService,
            IOptions<UrlShortenerSettings> settings,
            ILogger<UrlShortenerService> logger)
        {
            _urlRepository = urlRepository;
            _codeGenerator = codeGenerator;
            _cacheService = cacheService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<UrlShortenResponse> ShortenUrlAsync(UrlShortenRequest request)
        {
            if (string.IsNullOrEmpty(request.LongUrl))
            {
                throw new ArgumentException("URL is required", nameof(request.LongUrl));
            }

            // Nếu URL không bắt đầu bằng http hoặc https, thêm http://
            if (!request.LongUrl.StartsWith("http://") && !request.LongUrl.StartsWith("https://"))
            {
                request.LongUrl = "http://" + request.LongUrl;
            }

            if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Invalid URL format", nameof(request.LongUrl));
            }

            // Check if URL already exists
            var existingUrl = await _urlRepository.GetByOriginalUrlAsync(request.LongUrl);
            if (existingUrl != null)
            {
                _logger.LogInformation("Found existing URL for {LongUrl} with code {Code}", request.LongUrl, existingUrl.Code);
                return new UrlShortenResponse
                {
                    OriginalUrl = existingUrl.OriginalUrl,
                    Code = existingUrl.Code,
                    ShortUrl = BuildShortUrl(existingUrl.Code)
                };
            }

            // Generate or use custom code
            string code;
            if (!string.IsNullOrEmpty(request.CustomCode))
            {
                // Cho phép mã tùy chỉnh có độ dài bất kỳ và không cần kiểm tra ký tự
                var codeExists = await _urlRepository.GetByCodeAsync(request.CustomCode);
                if (codeExists != null)
                {
                    throw new InvalidOperationException("Custom code already exists");
                }
                code = request.CustomCode;
            }
            else
            {
                code = await _codeGenerator.GenerateUniqueCodeAsync();
            }

            // Create shortened URL
            var shortenedUrl = new ShortenedUrl
            {
                Code = code,
                OriginalUrl = request.LongUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_settings.DefaultExpirationDays),
                ClickCount = 0
            };

            // Save to database
            var result = await _urlRepository.CreateAsync(shortenedUrl);
            if (result == null)
            {
                throw new Exception("Failed to save URL");
            }

            // Add to cache
            await _cacheService.SetOriginalUrlAsync(code, request.LongUrl);

            _logger.LogInformation("Created new shortened URL for {LongUrl} with code {Code}", request.LongUrl, code);
            return new UrlShortenResponse
            {
                OriginalUrl = request.LongUrl,
                Code = code,
                ShortUrl = BuildShortUrl(code)
            };
        }

        public async Task<string> GetOriginalUrlAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Code is required", nameof(code));
            }

            _logger.LogInformation("Looking up URL for code: {Code}", code);

            // Try to get URL from cache first
            var cachedUrl = await _cacheService.GetOriginalUrlAsync(code);
            if (!string.IsNullOrEmpty(cachedUrl))
            {
                _logger.LogInformation("Found URL in cache: {Url}", cachedUrl);
                return cachedUrl;
            }

            // If not in cache, get from database
            var shortenedUrl = await _urlRepository.GetByCodeAsync(code);
            if (shortenedUrl == null)
            {
                _logger.LogWarning("URL not found for code: {Code}", code);
                return null;
            }

            // Check if URL is expired
            if (shortenedUrl.ExpiresAt.HasValue && shortenedUrl.ExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("URL expired for code: {Code}", code);
                return null;
            }

            // Update cache
            await _cacheService.SetOriginalUrlAsync(code, shortenedUrl.OriginalUrl);
            _logger.LogInformation("Found URL in database: {Url}", shortenedUrl.OriginalUrl);

            return shortenedUrl.OriginalUrl;
        }

        public async Task<bool> IncrementClickCountAsync(string code)
        {
            var shortenedUrl = await _urlRepository.GetByCodeAsync(code);
            if (shortenedUrl == null)
            {
                return false;
            }

            shortenedUrl.ClickCount++;
            return await _urlRepository.UpdateAsync(shortenedUrl);
        }

        private string BuildShortUrl(string code)
        {
            // Đảm bảo rằng BaseUrl không kết thúc bằng dấu / và code không bắt đầu bằng dấu /
            var baseUrl = _settings.BaseUrl.TrimEnd('/');
            var cleanCode = code.TrimStart('/');
            
            return $"{baseUrl}/{cleanCode}";
        }
    }

    public class UrlShortenerSettings
    {
        public string BaseUrl { get; set; }
        public int DefaultExpirationDays { get; set; } = 30;
    }
}