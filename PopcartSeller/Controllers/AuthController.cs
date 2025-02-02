using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PopcartSeller.Models;
using PopcartSeller.Services;
using Microsoft.Extensions.Configuration;

namespace PopcartSeller.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly string _userManagementBaseUrl;

    public AuthController(ILogger<AuthController> logger, IHttpClientFactory httpClientFactory, IAuthService authService, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _authService = authService;
        _userManagementBaseUrl = configuration["ApiSettings:UserManagementBaseUrl"] ?? throw new Exception("Base API URL not found in configuration");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;

        if (!string.IsNullOrEmpty(token))
        {
            var businessProfile = await _authService.GetUserProfile(token);
            return View("Index", businessProfile);
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Sendotp()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyPhone(string phone, string code)
    {
        var requestData = new
        {
            phone = phone,
            code = code
        };

        _httpClient.BaseAddress = new Uri(_userManagementBaseUrl);

        var content = JsonContent.Create(requestData);

        try
        {
            var response = await _httpClient.PostAsync("auth/verify-phone", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var verifyPhoneResponse = JsonConvert.DeserializeObject<VerifyPhoneResponse>(responseContent);

                HttpContext.Session.SetString("Token", verifyPhoneResponse.Data.Token);
                return await Index();
            }
            else
            {
                return BadRequest("Failed to verify phone. Please check the details and try again.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while verifying the phone.");
            return StatusCode(500, "Internal Server Error");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
