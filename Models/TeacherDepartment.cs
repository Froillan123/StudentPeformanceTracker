using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPeformanceTracker.Models;

public class TeacherDepartment
{
    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("TeacherId")]
    public Teacher Teacher { get; set; } = null!;

    [ForeignKey("DepartmentId")]
    public Department Department { get; set; } = null!;
}
