using StudentPeformanceTracker.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // TODO: Add seeding logic when models are created
        // if (context.Students.Any())
        //     return;
        // 
        // Add your seeding logic here...
    }
}
