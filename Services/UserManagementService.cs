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
}
