namespace UrlShortener.Core.DTOs
{
    public class UrlShortenRequest
    {
        public string LongUrl { get; set; } = string.Empty;
        public string CustomCode { get; set; } = string.Empty;
    }
}