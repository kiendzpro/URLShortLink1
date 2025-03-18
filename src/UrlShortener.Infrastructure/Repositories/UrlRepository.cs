using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories
{
    public class UrlRepository : IUrlRepository
    {
        private readonly UrlDbContext _dbContext;

        public UrlRepository(UrlDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ShortenedUrl> GetByCodeAsync(string code)
        {
            return await _dbContext.ShortenedUrls
                .FirstOrDefaultAsync(u => u.Code == code);
        }

        public async Task<ShortenedUrl> GetByOriginalUrlAsync(string originalUrl)
        {
            return await _dbContext.ShortenedUrls
                .FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl);
        }

        public async Task<ShortenedUrl> CreateAsync(ShortenedUrl shortenedUrl)
        {
            _dbContext.ShortenedUrls.Add(shortenedUrl);
            await _dbContext.SaveChangesAsync();
            return shortenedUrl;
        }

        public async Task<bool> UpdateAsync(ShortenedUrl shortenedUrl)
        {
            _dbContext.ShortenedUrls.Update(shortenedUrl);
            int rowsAffected = await _dbContext.SaveChangesAsync();
            return rowsAffected > 0;
        }
    }
}