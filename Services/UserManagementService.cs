using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Services;

public class UserManagementService
{
    private readonly IUserRepository _userRepository;

    public UserManagementService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        return await _userRepository.UpdateStatusAsync(userId, "Active");
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        return await _userRepository.UpdateStatusAsync(userId, "Inactive");
    }

    public async Task<IEnumerable<User>> GetPendingUsersAsync()
    {
        return await _userRepository.GetByStatusAsync("Inactive");
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetByStatusAsync("Active");
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetUsersFilteredAsync(
        int page,
        int pageSize,
        string? status = null,
        string? role = null,
        string? search = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null)
    {
        return await _userRepository.GetFilteredPaginatedAsync(page, pageSize, status, role, search, createdFrom, createdTo);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        return await _userRepository.DeleteAsync(userId);
    }
}
