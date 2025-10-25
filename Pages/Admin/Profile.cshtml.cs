using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentPeformanceTracker.Pages.Admin
{
    public class AdminProfileModel : PageModel
    {
        // No server-side API call needed - JavaScript handles it client-side
        // This makes the page load faster and uses browser cookies automatically
        public void OnGet()
        {
            // Page loads immediately, JavaScript fetches profile data
        }
    }
}
