using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using StudentPeformanceTracker.Configuration;

namespace StudentPeformanceTracker.Pages.Admin
{
    public class AdminProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiConfiguration _apiConfig;

        public AdminProfileModel(IHttpClientFactory httpClientFactory, ApiConfiguration apiConfig)
        {
            _httpClientFactory = httpClientFactory;
            _apiConfig = apiConfig;
        }

        public object? ProfileData { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("default");
                var response = await client.GetAsync($"{_apiConfig.BaseUrl}/api/v1/admin/profile");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ProfileData = JsonSerializer.Deserialize<object>(jsonContent);
                }
                else
                {
                    ErrorMessage = "Failed to load profile data";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading profile: {ex.Message}";
            }
        }
    }
}
