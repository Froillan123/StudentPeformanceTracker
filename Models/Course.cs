using System.ComponentModel.DataAnnotations;

namespace StudentPeformanceTracker.Models;

public class Course
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
