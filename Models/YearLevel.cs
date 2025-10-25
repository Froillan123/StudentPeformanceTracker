using System.ComponentModel.DataAnnotations;

namespace StudentPeformanceTracker.Models;

public class YearLevel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int LevelNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string LevelName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
