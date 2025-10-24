namespace StudentPeformanceTracker.DTO;

public class LoginRequest
{
    public string UsernameOrStudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
