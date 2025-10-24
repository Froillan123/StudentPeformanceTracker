using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _context.Courses.FindAsync(id);
    }

    public async Task<Course?> GetByCourseNameAsync(string courseName)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseName == courseName);
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public async Task<Course> CreateAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Courses.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> CourseNameExistsAsync(string courseName)
    {
        return await _context.Courses.AnyAsync(c => c.CourseName == courseName);
    }
}
