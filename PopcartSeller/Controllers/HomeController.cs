using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PopcartSeller.Models;
using PopcartSeller.Services;
using Newtonsoft.Json;
using System.Text;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly string _userManagementBaseUrl;

    public HomeController(ILogger<HomeController> logger, IAuthService authService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _authService = authService;
        _httpClient = httpClientFactory.CreateClient();
        _userManagementBaseUrl = configuration["ApiSettings:UserManagementBaseUrl"] ?? throw new Exception("Base API URL not found in configuration");
    }

    public async Task<IActionResult> AddProduct()
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;

        if (!string.IsNullOrEmpty(token))
        {
            var businessProfile = await _authService.GetUserProfile(token);
            var viewModel = new Main
            {
                BusinessProfile = businessProfile,
                Product = new Product()
            };
            return View("AddProduct", viewModel);
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }
    }
   public async Task<IActionResult> EditProduct(string id)
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;

        if (!string.IsNullOrEmpty(token))
        {
            var businessProfile = await _authService.GetUserProfile(token);
            var ProductDetail = await GetProduct(id);
            var viewModel = new Main
            {
                BusinessProfile = businessProfile,
                Product = new Product(),
                GetProduct = ProductDetail
            };
            return View("EditProduct", viewModel);
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }
    }
    

    public async Task<IActionResult> GetProductCategories()
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;
        string endpoint = $"{_userManagementBaseUrl}inventory/product-categories";

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return Content(responseData, "application/json");
            }

            return StatusCode((int)response.StatusCode, "Failed to fetch product categories.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error while fetching product categories.");
            return StatusCode(500, "An error occurred while fetching product categories.");
        }
    }
    public async Task<List<Inventory>> GetInventory()
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;
        string endpoint = $"{_userManagementBaseUrl}inventory";

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(responseData);

                if (inventoryResponse?.Data?.Products != null)
                {
                    return inventoryResponse.Data.Products;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error while fetching inventory.");
        }

        return new List<Inventory>();
    }
    public async Task<GetProduct> GetProduct(string id)
{
    string token = HttpContext.Session.GetString("Token") ?? string.Empty;
    string endpoint = $"{_userManagementBaseUrl}inventory/{id}"; 

    try
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            string responseData = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API Response: " + responseData);

            var productResponse = JsonConvert.DeserializeObject<ProductResponse>(responseData);

            if (productResponse == null)
            {
                _logger.LogError("Deserialization failed: productResponse is null.");
                return null;
            }

            if (productResponse.Data == null)
            {
                _logger.LogError("Deserialization failed: productResponse.Data is null.");
                return null;
            }

            return productResponse.Data;
        }
        else
        {
            _logger.LogError($"API request failed with status code: {response.StatusCode}");
        }
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Error while fetching product details.");
    }

    return null;
}

    public async Task<string> UploadPhoto(IFormFile file)
    {
        string endpoint = $"{_userManagementBaseUrl}upload";

        try
        {
            if (file == null || file.Length == 0)
            {
                return "No file was provided or the file is empty.";
            }

            using (var form = new MultipartFormDataContent())
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                form.Add(streamContent, "files", file.FileName);

                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, form);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return responseJson.data[0];
                }
                else
                {
                    return $"Failed to upload the photo. Status code: {response.StatusCode}";
                    
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading the photo.");
            return $"An error occurred while uploading the photo: {ex.Message}";
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(Main data)
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;
        var uploadedImageUrls = new List<string>();

        try
        {
            foreach (var image in data.Product.images)
            {
                if (image != null && image.Length > 0)
                {
                    var uploadedUrl = await UploadPhoto(image);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        uploadedImageUrls.Add(uploadedUrl);
                    }
                    else
                    {
                        TempData["Error"] = "Image upload failed for one or more images.";
                        return RedirectToAction(nameof(AddProduct));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving image URL.");
            TempData["Error"] = "An error occurred while retrieving image URL.";
            return RedirectToAction(nameof(AddProduct));
        }

        var requestData = new
        {
            name = data.Product.name,
            category = data.Product.category,
            description = data.Product.description,
            brand = data.Product.brand,
            stockUnit = data.Product.stockUnit,
            price = data.Product.price,
            images = uploadedImageUrls
        };

        using var client = new HttpClient();
        client.BaseAddress = new Uri(_userManagementBaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = JsonContent.Create(requestData);

        try
        {
            var response = await client.PostAsync("inventory/add-product", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product added successfully!";
                return RedirectToAction(nameof(AddProduct));
            }
            else
            {
                TempData["Error"] = "Failed to add product.";
                return RedirectToAction(nameof(AddProduct));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding the product.");
            TempData["Error"] = "An error occurred while adding the product.";
            return RedirectToAction(nameof(AddProduct));
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProduct(Main data)
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;
        var uploadedImageUrls = new List<string>();
       if (data.Product != null && data.Product.images != null)
        {
            try
            {
                foreach (var image in data.Product.images)
                {
                    if (image != null && image.Length > 0)
                    {
                        var uploadedUrl = await UploadPhoto(image);
                        if (!string.IsNullOrEmpty(uploadedUrl))
                        {
                            uploadedImageUrls.Add(uploadedUrl);
                        }
                        else
                        {
                            TempData["Error"] = "Image upload failed for one or more images.";
                            return RedirectToAction(nameof(EditProduct));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving image URL.");
                TempData["Error"] = "An error occurred while retrieving image URL.";
                return RedirectToAction(nameof(EditProduct));
            }
        }
        

        var requestData = new
        {
            name = data.Product.name,
            category = data.Product.category,
            description = data.Product.description,
            brand = data.Product.brand,
            stockUnit = data.Product.stockUnit,
            price = data.Product.price,
            images = uploadedImageUrls
        };

        using var client = new HttpClient();
        client.BaseAddress = new Uri(_userManagementBaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = JsonContent.Create(requestData);

        try
        {
            var response = await client.PostAsync($"inventory/{data.GetProduct._id}/edit", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(EditProduct));
            }
            else
            {
                TempData["Error"] = "Failed to update product.";
                return RedirectToAction(nameof(EditProduct));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding the product.");
            TempData["Error"] = "An error occurred while adding the product.";
            return RedirectToAction(nameof(EditProduct));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        string token = HttpContext.Session.GetString("Token") ?? string.Empty;
        if (!string.IsNullOrEmpty(token))
        {
            string endpoint = $"{_userManagementBaseUrl}inventory/{id}/delete";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Product deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete product.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error while deleting product.");
                TempData["Error"] = "An error occurred while deleting the product.";
            }
        }

        return RedirectToAction("Inventory");
    }

    public async Task<IActionResult> Inventory()
    {
         string token = HttpContext.Session.GetString("Token") ?? string.Empty;

        if (!string.IsNullOrEmpty(token))
        {
            var businessProfile = await _authService.GetUserProfile(token);
            var inventoryList = await GetInventory();
            var viewModel = new Main
            {
                BusinessProfile = businessProfile,
                Inventory = inventoryList
            };
        
            return View("Inventory", viewModel);
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
