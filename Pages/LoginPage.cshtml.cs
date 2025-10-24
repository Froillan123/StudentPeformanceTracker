using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StudentPeformanceTracker.Pages;

public class LoginPageModel : PageModel
{
    private readonly ILogger<LoginPageModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public LoginPageModel(ILogger<LoginPageModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet()
    {
        // Check for redirect message from unauthorized access
        if (Request.Query.ContainsKey("message"))
        {
            ErrorMessage = Request.Query["message"];
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required";
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("default");
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Prepare login request
            var loginRequest = new
            {
                usernameOrStudentId = Username,
                password = Password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the login API
            var response = await client.PostAsync($"{baseUrl}/api/v1/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                // Get cookies from response and set them in the current response
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    foreach (var cookie in cookies)
                    {
                        Response.Headers.Append("Set-Cookie", cookie);
                    }
                }

                // Parse response to get user role
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.User != null)
                {
                    var role = loginResponse.User.Role;

                    _logger.LogInformation($"User {Username} logged in successfully with role {role}");

                    // Redirect based on role
                    return role switch
                    {
                        "Admin" => RedirectToPage("/Admin/Dashboard"),
                        "Student" => RedirectToPage("/Student/Dashboard"),
                        "Teacher" => RedirectToPage("/Teacher/Dashboard"),
                        _ => RedirectToPage("/Index")
                    };
                }

                return RedirectToPage("/Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Login failed for user {Username}: {errorContent}");
                ErrorMessage = "Invalid username or password";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during login for user {Username}");
            ErrorMessage = "An error occurred during login. Please try again.";
            return Page();
        }
    }

    // DTOs for API response
    private class LoginResponse
    {
        public string? Message { get; set; }
        public UserInfo? User { get; set; }
    }

    private class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
