using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces;

public interface IGradeRepository
{
    Task<IEnumerable<Grade>> GetAllAsync();
    Task<Grade?> GetByIdAsync(int id);
    Task<IEnumerable<Grade>> GetByStudentSubjectIdAsync(int studentSubjectId);
    Task<IEnumerable<Grade>> GetByStudentIdAsync(int studentId);
    Task<Grade> CreateAsync(Grade grade);
    Task<Grade> UpdateAsync(Grade grade);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

