using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;
        private readonly ILogger<UrlController> _logger;

        public UrlController(
            IUrlShortenerService urlShortenerService,
            ILogger<UrlController> logger)
        {
            _urlShortenerService = urlShortenerService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ShortenUrl([FromBody] UrlShortenRequest request)
        {
            try
            {
                _logger.LogInformation("Received request to shorten URL: {LongUrl}", request?.LongUrl);
                var response = await _urlShortenerService.ShortenUrlAsync(request);
                _logger.LogInformation("URL shortened successfully: {ShortUrl}", response.ShortUrl);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid URL shortening request: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Custom code conflict: {Message}", ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shortening URL: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetOriginalUrl(string code)
        {
            _logger.LogInformation("Received request to get original URL for code: {Code}", code);
            var originalUrl = await _urlShortenerService.GetOriginalUrlAsync(code);
            if (originalUrl == null)
            {
                _logger.LogWarning("Original URL not found for code: {Code}", code);
                return NotFound();
            }

            // Asynchronously increment the click count without waiting for completion
            _ = _urlShortenerService.IncrementClickCountAsync(code);
            
            _logger.LogInformation("Found original URL: {OriginalUrl}", originalUrl);
            return Ok(new { originalUrl });
        }
    }

    [ApiController]
    [Route("s")]
    public class RedirectController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;
        private readonly ILogger<RedirectController> _logger;

        public RedirectController(
            IUrlShortenerService urlShortenerService,
            ILogger<RedirectController> logger)
        {
            _urlShortenerService = urlShortenerService;
            _logger = logger;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string code)
        {
            _logger.LogInformation("Received redirect request for code: {Code}", code);
            
            var originalUrl = await _urlShortenerService.GetOriginalUrlAsync(code);
            if (originalUrl == null)
            {
                _logger.LogWarning("Original URL not found for code: {Code}", code);
                return NotFound();
            }

            // Asynchronously increment the click count without waiting for completion
            _ = _urlShortenerService.IncrementClickCountAsync(code);
            
            _logger.LogInformation("Redirecting to: {OriginalUrl}", originalUrl);
            return Redirect(originalUrl);
        }
    }
}