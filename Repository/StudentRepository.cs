using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByUserIdAsync(int userId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<Student?> GetByStudentIdAsync(string studentId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Course)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByCourseIdAsync(int courseId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Course)
            .Where(s => s.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByYearLevelAsync(int yearLevel)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Course)
            .Where(s => s.YearLevel == yearLevel)
            .ToListAsync();
    }

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        student.UpdatedAt = DateTime.UtcNow;
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Students.AnyAsync(s => s.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Students.AnyAsync(s => s.Email == email);
    }

    public async Task<bool> StudentIdExistsAsync(string studentId)
    {
        return await _context.Students.AnyAsync(s => s.StudentId == studentId);
    }
}
