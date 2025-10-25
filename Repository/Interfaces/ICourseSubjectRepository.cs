using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface ICourseSubjectRepository
    {
        Task<IEnumerable<CourseSubject>> GetAllAsync();
        Task<CourseSubject?> GetByIdAsync(int id);
        Task<IEnumerable<CourseSubject>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<CourseSubject>> GetByCourseAndYearAsync(int courseId, int yearLevelId);
        Task<IEnumerable<CourseSubject>> GetByCourseYearSemesterAsync(int courseId, int yearLevelId, int semesterId);
        Task<CourseSubject> CreateAsync(CourseSubject courseSubject);
        Task<CourseSubject> UpdateAsync(CourseSubject courseSubject);
        Task<bool> DeleteAsync(int id);
    }
}
