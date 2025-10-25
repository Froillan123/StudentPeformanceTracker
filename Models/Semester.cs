using System.ComponentModel.DataAnnotations;

namespace StudentPeformanceTracker.Models;

public class Semester
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string SemesterName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string SemesterCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string SchoolYear { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
