using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentPeformanceTracker.Pages
{
    public class RegisterModel : PageModel
    {
        public void OnGet()
        {
        }
        
        public IActionResult OnPost()
        {
            // Simple redirect to login page
            return RedirectToPage("/LoginPage");
        }
    }
}