namespace StudentPeformanceTracker.DTO;

public class StudentRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Student-specific fields
    public string StudentNumber { get; set; } = string.Empty; // User inputs: 22686570, system generates: ucmn-22686570
    public int? YearLevel { get; set; }
    public int CourseId { get; set; } // Required for students
}
