namespace StudentPeformanceTracker.DTO;

public class TeacherRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? HighestQualification { get; set; }
    public string Status { get; set; } = "Full-time"; // Full-time or Part-time
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateTime? HireDate { get; set; }
    // Note: Departments will be assigned by admins after registration
}