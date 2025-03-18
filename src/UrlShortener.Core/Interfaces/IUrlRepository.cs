using System.Threading.Tasks;
using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Interfaces
{
    public interface IUrlRepository
    {
        Task<ShortenedUrl> GetByCodeAsync(string code);
        Task<ShortenedUrl> GetByOriginalUrlAsync(string originalUrl);
        Task<ShortenedUrl> CreateAsync(ShortenedUrl shortenedUrl);
        Task<bool> UpdateAsync(ShortenedUrl shortenedUrl);
    }
}