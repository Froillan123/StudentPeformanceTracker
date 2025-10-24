using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository;

public class TeacherRepository : ITeacherRepository
{
    private readonly AppDbContext _context;

    public TeacherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Teacher>> GetAllAsync()
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.TeacherDepartments)
                .ThenInclude(td => td.Department)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
    }

    public async Task<Teacher?> GetByIdAsync(int id)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.TeacherDepartments)
                .ThenInclude(td => td.Department)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Teacher?> GetByUserIdAsync(int userId)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.TeacherDepartments)
                .ThenInclude(td => td.Department)
            .FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task<Teacher?> GetByEmailAsync(string email)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.TeacherDepartments)
                .ThenInclude(td => td.Department)
            .FirstOrDefaultAsync(t => t.Email == email);
    }

    public async Task<IEnumerable<Teacher>> GetByDepartmentIdAsync(int departmentId)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.TeacherDepartments)
                .ThenInclude(td => td.Department)
            .Where(t => t.TeacherDepartments.Any(td => td.DepartmentId == departmentId))
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
    }

    public async Task<Teacher> CreateAsync(Teacher teacher)
    {
        teacher.CreatedAt = DateTime.UtcNow;
        teacher.UpdatedAt = DateTime.UtcNow;
        
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        return teacher;
    }

    public async Task<Teacher> UpdateAsync(Teacher teacher)
    {
        teacher.UpdatedAt = DateTime.UtcNow;
        
        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync();
        return teacher;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null)
            return false;

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Teachers.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Teachers.AnyAsync(t => t.Email == email);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Teachers.CountAsync();
    }

    public async Task<int> GetCountByDepartmentAsync(int departmentId)
    {
        return await _context.Teachers.CountAsync(t => t.TeacherDepartments.Any(td => td.DepartmentId == departmentId));
    }
}