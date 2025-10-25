using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Services
{
    public class CourseSubjectService
    {
        private readonly ICourseSubjectRepository _courseSubjectRepository;

        public CourseSubjectService(ICourseSubjectRepository courseSubjectRepository)
        {
            _courseSubjectRepository = courseSubjectRepository;
        }

        public async Task<IEnumerable<CourseSubjectDto>> GetAllAsync()
        {
            var courseSubjects = await _courseSubjectRepository.GetAllAsync();
            return courseSubjects.Select(MapToDto);
        }

        public async Task<CourseSubjectDto?> GetByIdAsync(int id)
        {
            var courseSubject = await _courseSubjectRepository.GetByIdAsync(id);
            return courseSubject != null ? MapToDto(courseSubject) : null;
        }

        public async Task<IEnumerable<CourseSubjectDto>> GetByCourseIdAsync(int courseId)
        {
            var courseSubjects = await _courseSubjectRepository.GetByCourseIdAsync(courseId);
            return courseSubjects.Select(MapToDto);
        }

        public async Task<IEnumerable<CourseSubjectDto>> GetByCourseAndYearAsync(int courseId, int yearLevelId)
        {
            var courseSubjects = await _courseSubjectRepository.GetByCourseAndYearAsync(courseId, yearLevelId);
            return courseSubjects.Select(MapToDto);
        }

        public async Task<IEnumerable<CourseSubjectDto>> GetByCourseYearSemesterAsync(int courseId, int yearLevelId, int semesterId)
        {
            var courseSubjects = await _courseSubjectRepository.GetByCourseYearSemesterAsync(courseId, yearLevelId, semesterId);
            return courseSubjects.Select(MapToDto);
        }

        public async Task<CourseSubjectDto> CreateAsync(CourseSubject courseSubject)
        {
            var created = await _courseSubjectRepository.CreateAsync(courseSubject);
            return MapToDto(created);
        }

        public async Task<CourseSubjectDto> CreateAsync(CreateCourseSubjectRequest request)
        {
            var courseSubject = new CourseSubject
            {
                CourseId = request.CourseId,
                SubjectId = request.SubjectId,
                YearLevelId = request.YearLevelId,
                SemesterId = request.SemesterId,
                IsRequired = request.IsRequired
            };

            var created = await _courseSubjectRepository.CreateAsync(courseSubject);
            return MapToDto(created);
        }

        public async Task<CourseSubjectDto> UpdateAsync(CourseSubject courseSubject)
        {
            var updated = await _courseSubjectRepository.UpdateAsync(courseSubject);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _courseSubjectRepository.DeleteAsync(id);
        }

        private static CourseSubjectDto MapToDto(CourseSubject courseSubject)
        {
            return new CourseSubjectDto
            {
                Id = courseSubject.Id,
                CourseId = courseSubject.CourseId,
                CourseName = courseSubject.Course?.CourseName ?? string.Empty,
                SubjectId = courseSubject.SubjectId,
                SubjectName = courseSubject.Subject?.SubjectName ?? string.Empty,
                Units = courseSubject.Subject?.Units ?? 0,
                YearLevelId = courseSubject.YearLevelId,
                YearLevelName = courseSubject.YearLevel?.LevelName ?? string.Empty,
                SemesterId = courseSubject.SemesterId,
                SemesterName = courseSubject.Semester?.SemesterName ?? string.Empty,
                SchoolYear = courseSubject.Semester?.SchoolYear ?? string.Empty,
                IsRequired = courseSubject.IsRequired,
                CreatedAt = courseSubject.CreatedAt,
                UpdatedAt = courseSubject.UpdatedAt
            };
        }
    }

    public class CreateCourseSubjectRequest
    {
        public int CourseId { get; set; }
        public int SubjectId { get; set; }
        public int YearLevelId { get; set; }
        public int SemesterId { get; set; }
        public bool IsRequired { get; set; } = true;
    }
}
