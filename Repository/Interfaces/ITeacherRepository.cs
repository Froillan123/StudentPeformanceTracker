using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces;

public interface ITeacherRepository
{
    Task<IEnumerable<Teacher>> GetAllAsync();
    Task<Teacher?> GetByIdAsync(int id);
    Task<Teacher?> GetByUserIdAsync(int userId);
    Task<Teacher?> GetByEmailAsync(string email);
    Task<IEnumerable<Teacher>> GetByDepartmentIdAsync(int departmentId);
    Task<Teacher> CreateAsync(Teacher teacher);
    Task<Teacher> UpdateAsync(Teacher teacher);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<int> GetCountAsync();
    Task<int> GetCountByDepartmentAsync(int departmentId);
    Task<(IEnumerable<Teacher> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
}