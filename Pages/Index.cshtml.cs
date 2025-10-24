using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentPeformanceTracker.Pages;

public class IndexModel : PageModel
{
    // TODO: Add database context when PostgreSQL is set up
    // private readonly AppDbContext _context;
    //
    // public IndexModel(AppDbContext context)
    // {
    //     _context = context;
    // }

    // TODO: Add properties when models are created
    // public IList<Student> Students { get; set; } = default!;

    public void OnGet()
    {
        // TODO: Add data loading when models are created
        // Students = await _context.Students
        //     .Include(s => s.StudentSubjects)
        //         .ThenInclude(ss => ss.Subject)
        //     .Include(s => s.Course)
        //     .ToListAsync();
    }
}
