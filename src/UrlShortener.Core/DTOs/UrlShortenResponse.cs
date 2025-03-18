namespace UrlShortener.Core.DTOs
{
    public class UrlShortenResponse
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}