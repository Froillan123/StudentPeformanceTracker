using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using StudentPeformanceTracker.Configuration;

namespace StudentPeformanceTracker.Pages;

public class LoginPageModel : PageModel
{
    private readonly ILogger<LoginPageModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiConfiguration _apiConfig;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public LoginPageModel(ILogger<LoginPageModel> logger, IHttpClientFactory httpClientFactory, ApiConfiguration apiConfig)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _apiConfig = apiConfig;
    }

    public void OnGet()
    {
        // Check for redirect message from unauthorized access or registration
        if (Request.Query.ContainsKey("message"))
        {
            var message = Request.Query["message"].ToString();
            // Check if it's a registration success message
            if (message.Contains("Registration successful") || message.Contains("pending admin approval"))
            {
                SuccessMessage = message;
            }
            else
            {
                ErrorMessage = message;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username/Email and password are required";
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("default");

            // Prepare login request
            var loginRequest = new
            {
                usernameOrStudentId = Username,
                password = Password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the login API using ApiConfiguration
            var response = await client.PostAsync(_apiConfig.LoginEndpoint, content);

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
                
                // Check if it's an inactive account error
                if (errorContent.Contains("ACCOUNT_PENDING") || errorContent.Contains("pending") || errorContent.Contains("activation"))
                {
                    ErrorMessage = "PENDING_APPROVAL:Your account is pending admin approval. Please contact an administrator to activate your account.";
                }
                else
                {
                    ErrorMessage = "Invalid username/email or password";
                }
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
