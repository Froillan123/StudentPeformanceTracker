using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task<IEnumerable<Enrollment>> GetAllAsync();
        Task<Enrollment?> GetByIdAsync(int id);
        Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId);
        Task<Enrollment?> GetByStudentCourseYearSemesterAsync(int studentId, int courseId, int yearLevelId, int semesterId);
        Task<Enrollment> CreateAsync(Enrollment enrollment);
        Task<Enrollment> UpdateAsync(Enrollment enrollment);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<Enrollment> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    }
}
