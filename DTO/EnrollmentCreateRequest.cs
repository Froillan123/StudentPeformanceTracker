using System.ComponentModel.DataAnnotations;

namespace StudentPeformanceTracker.DTO
{
    public class EnrollmentCreateRequest
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int YearLevelId { get; set; }

        [Required]
        public int SemesterId { get; set; }

        [Required]
        [StringLength(20)]
        public string EnrollmentType { get; set; } = "Regular";
    }
}
