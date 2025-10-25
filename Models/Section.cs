using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class Section
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string SectionName { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }

    [Required]
    public int YearLevelId { get; set; }

    [Required]
    public int SemesterId { get; set; }

    [Required]
    public int MaxCapacity { get; set; } = 40;

    public int CurrentEnrollment { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CourseId")]
    public Course Course { get; set; } = null!;

    [ForeignKey("YearLevelId")]
    public YearLevel YearLevel { get; set; } = null!;

    [ForeignKey("SemesterId")]
    public Semester Semester { get; set; } = null!;

    public ICollection<SectionSubject> SectionSubjects { get; set; } = new List<SectionSubject>();
}
