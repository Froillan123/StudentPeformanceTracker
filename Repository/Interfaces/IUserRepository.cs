using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameOrStudentIdAsync(string usernameOrStudentId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> UpdateStatusAsync(int userId, string status);
    Task<IEnumerable<User>> GetByStatusAsync(string status);
    Task<int> GetCountAsync();
    Task<(IEnumerable<User> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, string? status = null);
    Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredPaginatedAsync(
        int page,
        int pageSize,
        string? status = null,
        string? role = null,
        string? search = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null);
}
