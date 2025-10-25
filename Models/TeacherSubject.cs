using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class TeacherSubject
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int SectionSubjectId { get; set; }

    public bool IsPrimary { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("TeacherId")]
    public Teacher Teacher { get; set; } = null!;

    [ForeignKey("SectionSubjectId")]
    public SectionSubject SectionSubject { get; set; } = null!;
}
