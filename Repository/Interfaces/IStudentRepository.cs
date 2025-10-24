using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByUserIdAsync(int userId);
    Task<Student?> GetByStudentIdAsync(string studentId);
    Task<Student?> GetByEmailAsync(string email);
    Task<IEnumerable<Student>> GetAllAsync();
    Task<IEnumerable<Student>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<Student>> GetByYearLevelAsync(int yearLevel);
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> StudentIdExistsAsync(string studentId);
}
