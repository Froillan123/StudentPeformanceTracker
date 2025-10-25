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
    public string AssessmentType { get; set; } = string.Empty; // Quiz, Exam, Project, Assignment

    [Required]
    [MaxLength(200)]
    public string AssessmentName { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Score { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal MaxScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Percentage { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public DateTime? DateGiven { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("StudentSubjectId")]
    public StudentSubject StudentSubject { get; set; } = null!;
}

