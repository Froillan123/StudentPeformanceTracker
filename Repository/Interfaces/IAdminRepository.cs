using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces;

public interface IAdminRepository
{
    Task<Admin?> GetByIdAsync(int id);
    Task<Admin?> GetByUserIdAsync(int userId);
    Task<Admin?> GetByEmailAsync(string email);
    Task<IEnumerable<Admin>> GetAllAsync();
    Task<Admin> CreateAsync(Admin admin);
    Task<Admin> UpdateAsync(Admin admin);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email);
}
