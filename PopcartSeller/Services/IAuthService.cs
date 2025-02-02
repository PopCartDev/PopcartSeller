using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PopcartSeller.Models;

namespace PopcartSeller.Services
{
    public interface IAuthService
    {
        Task<BusinessProfile> GetUserProfile(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;
        private readonly string _userManagementBaseUrl;

        public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _userManagementBaseUrl = configuration["ApiSettings:UserManagementBaseUrl"] ?? throw new Exception("Base API URL not found in configuration");
            _logger = logger;
        }
        

        public async Task<BusinessProfile> GetUserProfile(string token)
        {
            string endpoint = $"{_userManagementBaseUrl}user/profile";
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token is null or empty.");
                return null;
            }

            try
            {
                // Add the Authorization header with the Bearer token
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var businessProfile = JsonConvert.DeserializeObject<BusinessProfile>(responseContent);

                    if (businessProfile == null)
                    {
                        _logger.LogWarning("Failed to deserialize the business profile.");
                        return null;
                    }

                    // Determine the greeting
                    var currentHour = DateTime.Now.Hour;
                    string greeting = currentHour switch
                    {
                        < 12 => "Good Morning",
                        >= 12 and < 18 => "Good Afternoon",
                        _ => "Good Evening"
                    };

                    // Append greeting and token
                    businessProfile.Greeting = $"{greeting} {businessProfile.data?.Username.ToUpper()}";
                    businessProfile.Token = token;

                    return businessProfile;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve user profile. StatusCode: {StatusCode}, Reason: {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return null; // Handle the error appropriately
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user profile.");
                return null;
            }
        }
    }
}
