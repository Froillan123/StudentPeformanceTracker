using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByUsernameOrStudentIdAsync(string usernameOrStudentId)
    {
        return await _context.Users
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u => u.Username == usernameOrStudentId ||
                (u.Student != null && u.Student.StudentId == usernameOrStudentId));
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> UpdateStatusAsync(int userId, string status)
    {
        if (status != "Active" && status != "Inactive")
            return false;

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetByStatusAsync(string status)
    {
        return await _context.Users
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .Where(u => u.Status == status)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Users.CountAsync();
    }
}
