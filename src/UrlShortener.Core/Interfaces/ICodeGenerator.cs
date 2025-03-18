using System.Threading.Tasks;

namespace UrlShortener.Core.Interfaces
{
    public interface ICodeGenerator
    {
        Task<string> GenerateUniqueCodeAsync();
        bool IsValidCode(string code);
    }
}