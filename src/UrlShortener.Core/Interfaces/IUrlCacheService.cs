using System.Threading.Tasks;

namespace UrlShortener.Core.Interfaces
{
    public interface IUrlCacheService
    {
        Task<string> GetOriginalUrlAsync(string code);
        Task SetOriginalUrlAsync(string code, string originalUrl);
        Task RemoveAsync(string code);
    }
}