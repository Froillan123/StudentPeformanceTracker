using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class Announcement
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int SectionSubjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "General"; // General, Important, Urgent

    [Required]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("TeacherId")]
    public Teacher Teacher { get; set; } = null!;

    [ForeignKey("SectionSubjectId")]
    public SectionSubject SectionSubject { get; set; } = null!;
}

