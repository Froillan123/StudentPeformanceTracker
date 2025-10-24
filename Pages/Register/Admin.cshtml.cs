using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentPeformanceTracker.Configuration;

namespace StudentPeformanceTracker.Pages.Register;

public class AdminModel : PageModel
{
    private readonly ApiConfiguration _apiConfig;

    public AdminModel(ApiConfiguration apiConfig)
    {
        _apiConfig = apiConfig;
    }

    [BindProperty]
    public string SecretPassword { get; set; } = string.Empty;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string? Phone { get; set; }

    public string? ErrorMessage { get; set; }
    public bool IsSecretPasswordValid { get; set; } = false;

    public void OnGet()
    {
        // Initialize page
    }

    public IActionResult OnPostValidateSecret()
    {
        try
        {
            // Get admin registration password from environment
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_REGISTRATION_PASSWORD");
            
            if (string.IsNullOrEmpty(adminPassword))
            {
                ErrorMessage = "Admin registration is not configured.";
                return Page();
            }

            if (SecretPassword != adminPassword)
            {
                ErrorMessage = "Invalid admin registration password.";
                return Page();
            }

            // Secret password is valid, show registration form
            IsSecretPasswordValid = true;
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        try
        {
            // Validate passwords match
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                IsSecretPasswordValid = true; // Keep showing the form
                return Page();
            }

            // Validate required fields
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Email) || 
                string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(FirstName) || 
                string.IsNullOrEmpty(LastName))
            {
                ErrorMessage = "Please fill in all required fields.";
                IsSecretPasswordValid = true; // Keep showing the form
                return Page();
            }

            // Create registration request
            var registrationData = new
            {
                Username = Username,
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                Phone = Phone,
                Role = "Admin"
            };

            // Call API to register admin
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(_apiConfig.RegisterAdminEndpoint, registrationData);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/LoginPage", new { message = "Admin registration successful! Please login with your credentials." });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Registration failed: {errorContent}";
                IsSecretPasswordValid = true; // Keep showing the form
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            IsSecretPasswordValid = true; // Keep showing the form
            return Page();
        }
    }
}
