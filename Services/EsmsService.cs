
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace RealTimeLeaderboard.Services
{
    public class EsmsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // HttpClient is injected automatically by .NET 8
        public EsmsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
        {
            // 1. Pull credentials from your secure appsettings.json
            var apiKey = _configuration["eSMS:ApiKey"];
            var secretKey = _configuration["eSMS:SecretKey"];
            var brandName = _configuration["eSMS:Brandname"];

            // 2. Construct the exact URL required by eSMS
            string content = $"Your leaderboard verification code is: {otpCode}";
            string requestUrl = $"http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get" +
                                $"?Phone={phoneNumber}" +
                                $"&Content={content}" +
                                $"&ApiKey={apiKey}" +
                                $"&SecretKey={secretKey}" +
                                $"&Brandname={brandName}" +
                                $"&Sandbox = 1" +
                                $"&SmsType=2"; // 2 = Customer Care Message

            // 3. Make the HTTP GET request
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"eSMS response: {jsonResponse}");
                // eSMS returns a CodeResult of "100" for success
                if (jsonResponse.Contains("\"CodeResult\":\"100\""))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

