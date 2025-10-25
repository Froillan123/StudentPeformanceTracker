using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class StudentSubject
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public int SectionSubjectId { get; set; }

    [Required]
    public int EnrollmentId { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Grade { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Enrolled"; // "Enrolled", "Completed", "Dropped", "Failed"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("StudentId")]
    public Student Student { get; set; } = null!;

    [ForeignKey("SectionSubjectId")]
    public SectionSubject SectionSubject { get; set; } = null!;

    [ForeignKey("EnrollmentId")]
    public Enrollment Enrollment { get; set; } = null!;
}
