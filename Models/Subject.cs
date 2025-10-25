using System.ComponentModel.DataAnnotations;

namespace StudentPeformanceTracker.Models;

public class Subject
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SubjectName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int Units { get; set; } = 3;

    [MaxLength(500)]
    public string? Prerequisites { get; set; }

    public bool IsActive { get; set; } = true;

    public int? CourseId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Course? Course { get; set; }
    public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
    public ICollection<SectionSubject> SectionSubjects { get; set; } = new List<SectionSubject>();
}
