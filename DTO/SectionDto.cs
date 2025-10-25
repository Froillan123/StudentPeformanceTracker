namespace StudentPeformanceTracker.DTO
{
    public class SectionDto
    {
        public int Id { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int YearLevelId { get; set; }
        public string YearLevelName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public int CurrentEnrollment { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
