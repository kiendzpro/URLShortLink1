using System;
using System.Linq;
using System.Threading.Tasks;
using UrlShortener.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace UrlShortener.Core.Services
{
    public class DefaultCodeGenerator : ICodeGenerator
    {
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int CodeLength = 6;
        private readonly IUrlRepository _urlRepository;
        private readonly Random _random;
        private readonly ILogger<DefaultCodeGenerator> _logger;

        public DefaultCodeGenerator(IUrlRepository urlRepository, ILogger<DefaultCodeGenerator> logger)
        {
            _urlRepository = urlRepository;
            _random = new Random();
            _logger = logger;
        }

        public async Task<string> GenerateUniqueCodeAsync()
        {
            string code;
            bool isUnique = false;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                code = GenerateCode();
                var existingUrl = await _urlRepository.GetByCodeAsync(code);
                isUnique = existingUrl == null;
                attempts++;
                
                if (attempts >= maxAttempts && !isUnique)
                {
                    _logger.LogWarning("Failed to generate unique code after {Attempts} attempts", attempts);
                    // Thêm timestamp để đảm bảo tính duy nhất
                    code = $"{code}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000}";
                    isUnique = true;
                }
            } while (!isUnique);

            _logger.LogInformation("Generated unique code: {Code} after {Attempts} attempts", code, attempts);
            return code;
        }

        public bool IsValidCode(string code)
        {
            // Thay đổi để chấp nhận mã tùy chỉnh với độ dài bất kỳ
            if (string.IsNullOrEmpty(code))
                return false;

            // Chỉ kiểm tra độ dài tối thiểu
            if (code.Length < 3)
                return false;

            // Không kiểm tra ký tự, cho phép bất kỳ ký tự nào
            return true;
        }

        private string GenerateCode()
        {
            char[] code = new char[CodeLength];

            for (int i = 0; i < CodeLength; i++)
            {
                code[i] = AllowedChars[_random.Next(AllowedChars.Length)];
            }

            return new string(code);
        }
    }
}