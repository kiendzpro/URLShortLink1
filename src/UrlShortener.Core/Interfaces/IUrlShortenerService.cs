using System.Threading.Tasks;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Interfaces
{
    public interface IUrlShortenerService
    {
        Task<UrlShortenResponse> ShortenUrlAsync(UrlShortenRequest request);
        Task<string> GetOriginalUrlAsync(string code);
        Task<bool> IncrementClickCountAsync(string code);
    }
}