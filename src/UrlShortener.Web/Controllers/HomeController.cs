using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public HomeController(
            ILogger<HomeController> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<ApiSettings> apiSettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        public IActionResult Index()
        {
            return View(new UrlShortenViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Shorten(UrlShortenViewModel model)
        {
            if (string.IsNullOrEmpty(model.LongUrl))
            {
                model.ErrorMessage = "URL is required";
                return View("Index", model);
            }

            try
            {
                // Log thêm thông tin để debug
                _logger.LogInformation("Sending request to API: {ApiUrl} with URL: {LongUrl}", 
                    $"{_apiSettings.ApiBaseUrl}/api/url", model.LongUrl);
                
                var client = _httpClientFactory.CreateClient("ApiClient");
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var requestData = new
                {
                    longUrl = model.LongUrl,
                    customCode = model.CustomCode ?? string.Empty
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                _logger.LogInformation("Request content: {Content}", jsonContent);

                var content = new StringContent(
                    jsonContent,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync($"{_apiSettings.ApiBaseUrl}/api/url", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<UrlShortenResponseModel>(responseContent);
                    if (result != null)
                    {
                        model.ShortUrl = result.ShortUrl;
                        model.ErrorMessage = null;
                    }
                    else
                    {
                        model.ErrorMessage = "Unable to parse API response";
                    }
                }
                else
                {
                    try
                    {
                        var error = JsonConvert.DeserializeObject<ErrorResponseModel>(responseContent);
                        model.ErrorMessage = error?.Error ?? "Failed to shorten URL";
                    }
                    catch
                    {
                        model.ErrorMessage = $"API Error: {response.StatusCode} - {responseContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shortening URL");
                model.ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return View("Index", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ApiSettings
    {
        public string ApiBaseUrl { get; set; } = string.Empty;
    }

    // Models for serialization/deserialization
    public class UrlShortenResponseModel
    {
        [JsonProperty("originalUrl")]
        public string OriginalUrl { get; set; }
        
        [JsonProperty("shortUrl")]
        public string ShortUrl { get; set; }
        
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class ErrorResponseModel
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}