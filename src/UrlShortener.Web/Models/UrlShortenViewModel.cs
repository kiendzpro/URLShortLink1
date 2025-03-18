namespace UrlShortener.Web.Models
{
    public class UrlShortenViewModel
    {
        public string LongUrl { get; set; } = string.Empty;
        public string CustomCode { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}