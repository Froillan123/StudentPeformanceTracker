using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class Enrollment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public int YearLevelId { get; set; }

    [Required]
    public int SemesterId { get; set; }

    [Required]
    [MaxLength(20)]
    public string EnrollmentType { get; set; } = "Regular"; // "Regular" or "Irregular"

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // "Active", "Completed", "Dropped"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("StudentId")]
    public Student Student { get; set; } = null!;

    [ForeignKey("CourseId")]
    public Course Course { get; set; } = null!;

    [ForeignKey("YearLevelId")]
    public YearLevel YearLevel { get; set; } = null!;

    [ForeignKey("SemesterId")]
    public Semester Semester { get; set; } = null!;

    public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
}
