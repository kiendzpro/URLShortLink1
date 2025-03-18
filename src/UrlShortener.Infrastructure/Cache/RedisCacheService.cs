using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Infrastructure.Cache
{
    public class RedisCacheService : IUrlCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            };
        }

        public async Task<string> GetOriginalUrlAsync(string code)
        {
            return await _cache.GetStringAsync(GetCacheKey(code));
        }

        public async Task SetOriginalUrlAsync(string code, string originalUrl)
        {
            await _cache.SetStringAsync(GetCacheKey(code), originalUrl, _cacheOptions);
        }

        public async Task RemoveAsync(string code)
        {
            await _cache.RemoveAsync(GetCacheKey(code));
        }

        private string GetCacheKey(string code)
        {
            return $"url:{code}";
        }
    }
}