using System.ComponentModel.DataAnnotations;

namespace StudentPeformanceTracker.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Student"; // Student, Teacher, Admin

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Inactive"; // Active, Inactive

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
    public Admin? Admin { get; set; }
}
