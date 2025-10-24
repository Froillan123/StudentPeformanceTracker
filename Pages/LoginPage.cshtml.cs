using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentPeformanceTracker.Pages;

public class LoginPageModel : PageModel
{
    private readonly ILogger<LoginPageModel> _logger;
    public LoginPageModel(ILogger<LoginPageModel> logger)
    {
        _logger = logger;
    }
    public void OnGet()
    {
    }
}