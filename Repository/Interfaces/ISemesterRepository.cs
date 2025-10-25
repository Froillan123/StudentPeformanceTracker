using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface ISemesterRepository
    {
        Task<IEnumerable<Semester>> GetAllAsync();
        Task<Semester?> GetByIdAsync(int id);
        Task<Semester?> GetBySemesterCodeAsync(string semesterCode);
        Task<IEnumerable<Semester>> GetActiveAsync();
        Task<Semester> CreateAsync(Semester semester);
        Task<Semester> UpdateAsync(Semester semester);
        Task<bool> DeleteAsync(int id);
    }
}
