using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentPeformanceTracker.Pages.Register;

public class TeacherModel : PageModel
{
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

    [BindProperty]
    public int DepartmentId { get; set; }

    [BindProperty]
    public DateTime? HireDate { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Initialize page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Validate passwords match
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            // Validate required fields
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Email) || 
                string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(FirstName) || 
                string.IsNullOrEmpty(LastName) || DepartmentId == 0)
            {
                ErrorMessage = "Please fill in all required fields.";
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
                DepartmentId = DepartmentId,
                HireDate = HireDate,
                Role = "Teacher"
            };

            // Call API to register teacher
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync("https://localhost:5199/api/v1/auth/register/teacher", registrationData);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/LoginPage", new { message = "Teacher registration successful! Please login with your credentials." });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Registration failed: {errorContent}";
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            return Page();
        }
    }
}
