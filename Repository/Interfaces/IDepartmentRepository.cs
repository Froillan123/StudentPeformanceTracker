using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department?> GetByNameAsync(string departmentName);
    Task<Department?> GetByCodeAsync(string departmentCode);
    Task<Department> CreateAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string departmentName);
    Task<bool> ExistsByCodeAsync(string departmentCode);
    Task<int> GetCountAsync();
}
