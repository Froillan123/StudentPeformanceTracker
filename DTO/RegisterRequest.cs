namespace StudentPeformanceTracker.DTO;

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";

    // Common fields for all roles
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Student-specific fields
    public string? StudentNumber { get; set; } // Optional - deprecated, student ID is now auto-generated
    public int? YearLevel { get; set; }
    public int? CourseId { get; set; }

    // Teacher-specific fields
    public string? Department { get; set; }
}
