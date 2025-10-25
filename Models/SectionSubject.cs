using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class SectionSubject
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SectionId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    [MaxLength(20)]
    public string EdpCode { get; set; } = string.Empty;

    public int? TeacherId { get; set; }

    [MaxLength(20)]
    public string? ScheduleDay { get; set; }

    [MaxLength(50)]
    public string? ScheduleTime { get; set; }

    [MaxLength(50)]
    public string? Room { get; set; }

    public int MaxStudents { get; set; } = 40;

    public int CurrentEnrollment { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SectionId")]
    public Section Section { get; set; } = null!;

    [ForeignKey("SubjectId")]
    public Subject Subject { get; set; } = null!;

    [ForeignKey("TeacherId")]
    public Teacher? Teacher { get; set; }

    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
}
