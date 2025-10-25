using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentPeformanceTracker.Configuration;

namespace StudentPeformanceTracker.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApiConfiguration _apiConfig;

        public RegisterModel(ApiConfiguration apiConfig)
        {
            _apiConfig = apiConfig;
        }

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        public string? MiddleName { get; set; }

        [BindProperty]
        public DateTime? DateOfBirth { get; set; }

        [BindProperty]
        public int DepartmentId { get; set; }

        [BindProperty]
        public int CourseId { get; set; }

        [BindProperty]
        public string YearLevel { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string ContactNumber { get; set; } = string.Empty;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
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
                if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || 
                    string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Username) || 
                    string.IsNullOrEmpty(Password) || CourseId == 0 || 
                    string.IsNullOrEmpty(YearLevel))
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
                    Phone = ContactNumber,
                    StudentNumber = Username, // Use username as student number
                    YearLevel = int.Parse(YearLevel.Split(' ')[0]), // Extract number from "1st Year"
                    CourseId = CourseId, // Use selected course ID
                    Role = "Student"
                };

                // Call API to register student
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsJsonAsync(_apiConfig.RegisterStudentEndpoint, registrationData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/LoginPage", new { message = "Student registration successful! Please login with your credentials." });
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
}