using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class Grade
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentSubjectId { get; set; }

    [Required]
    [MaxLength(50)]
    public string AssessmentType { get; set; } = string.Empty; // Only "Midterm" or "Final Grade"

    [MaxLength(200)]
    public string? AssessmentName { get; set; } // Optional, auto-filled as "Midterm Grade" or "Final Grade"

    [Required]
    [Column(TypeName = "decimal(3,2)")]
    [Range(1.0, 5.0, ErrorMessage = "Grade point must be between 1.0 and 5.0")]
    public decimal GradePoint { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public DateTime? DateGiven { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("StudentSubjectId")]
    public StudentSubject StudentSubject { get; set; } = null!;
}

