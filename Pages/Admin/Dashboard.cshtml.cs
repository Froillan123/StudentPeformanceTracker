using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentPeformanceTracker.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardModel : PageModel
    {
        public void OnGet()
        {
            // Empty method for frontend display
            // In real application, you would fetch admin data, statistics, 
            // student and teacher information here
        }
    }
}
