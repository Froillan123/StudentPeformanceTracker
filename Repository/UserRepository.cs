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
                .ThenInclude(s => s!.Course)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s!.Course)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByUsernameOrStudentIdAsync(string usernameOrStudentId)
    {
        return await _context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s!.Course)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Username == usernameOrStudentId ||
                (u.Student != null && u.Student.Email == usernameOrStudentId) ||
                (u.Teacher != null && u.Teacher.Email == usernameOrStudentId) ||
                (u.Admin != null && u.Admin.Email == usernameOrStudentId));
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s!.Course)
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
                .ThenInclude(s => s!.Course)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .Where(u => u.Status == status)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, string? status = null)
    {
        var query = _context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s!.Course)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(u => u.Status == status);
        }
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (items, totalCount);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredPaginatedAsync(
        int page,
        int pageSize,
        string? status = null,
        string? role = null,
        string? search = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null)
    {
        var query = _context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s!.Course)
            .Include(u => u.Teacher)
            .Include(u => u.Admin)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(u => u.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role == role);
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= createdTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowered = search.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(lowered) ||
                // search by role-specific email and names
                (u.Student != null && (
                    (u.Student.Email != null && u.Student.Email.ToLower().Contains(lowered)) ||
                    (u.Student.FirstName != null && u.Student.FirstName.ToLower().Contains(lowered)) ||
                    (u.Student.LastName != null && u.Student.LastName.ToLower().Contains(lowered)) ||
                    (u.Student.StudentId != null && u.Student.StudentId.ToLower().Contains(lowered))
                )) ||
                (u.Teacher != null && (
                    (u.Teacher.Email != null && u.Teacher.Email.ToLower().Contains(lowered)) ||
                    (u.Teacher.FirstName != null && u.Teacher.FirstName.ToLower().Contains(lowered)) ||
                    (u.Teacher.LastName != null && u.Teacher.LastName.ToLower().Contains(lowered))
                )) ||
                (u.Admin != null && (
                    (u.Admin.Email != null && u.Admin.Email.ToLower().Contains(lowered)) ||
                    (u.Admin.FirstName != null && u.Admin.FirstName.ToLower().Contains(lowered)) ||
                    (u.Admin.LastName != null && u.Admin.LastName.ToLower().Contains(lowered))
                ))
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .ThenBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
