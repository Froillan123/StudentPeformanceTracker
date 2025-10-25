using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository;

public class GradeRepository : IGradeRepository
{
    private readonly AppDbContext _context;

    public GradeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        return await _context.Grades
            .Include(g => g.StudentSubject)
                .ThenInclude(ss => ss.Student)
            .Include(g => g.StudentSubject)
                .ThenInclude(ss => ss.SectionSubject)
                    .ThenInclude(secSub => secSub.Subject)
            .OrderByDescending(g => g.DateGiven ?? g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Grade?> GetByIdAsync(int id)
    {
        return await _context.Grades
            .Include(g => g.StudentSubject)
                .ThenInclude(ss => ss.Student)
            .Include(g => g.StudentSubject)
                .ThenInclude(ss => ss.SectionSubject)
                    .ThenInclude(secSub => secSub.Subject)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Grade>> GetByStudentSubjectIdAsync(int studentSubjectId)
    {
        return await _context.Grades
            .Include(g => g.StudentSubject)
                .ThenInclude(ss => ss.SectionSubject)
                    .ThenInclude(secSub => secSub.Subject)
            .Where(g => g.StudentSubjectId == studentSubjectId)
            .OrderByDescending(g => g.DateGiven ?? g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Grades
            .Include(g => g.StudentSubject)
                .ThenInclude(ss => ss.SectionSubject)
                    .ThenInclude(secSub => secSub.Subject)
            .Include(g => g.StudentSubject)
                .ThenInclude(ss => ss.SectionSubject)
                    .ThenInclude(secSub => secSub.Section)
            .Where(g => g.StudentSubject.StudentId == studentId)
            .OrderByDescending(g => g.DateGiven ?? g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Grade> CreateAsync(Grade grade)
    {
        // Auto-calculate percentage
        if (grade.MaxScore > 0)
        {
            grade.Percentage = Math.Round((grade.Score / grade.MaxScore) * 100, 2);
        }

        grade.CreatedAt = DateTime.UtcNow;
        grade.UpdatedAt = DateTime.UtcNow;

        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task<Grade> UpdateAsync(Grade grade)
    {
        // Recalculate percentage
        if (grade.MaxScore > 0)
        {
            grade.Percentage = Math.Round((grade.Score / grade.MaxScore) * 100, 2);
        }

        grade.UpdatedAt = DateTime.UtcNow;

        _context.Grades.Update(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade == null)
            return false;

        _context.Grades.Remove(grade);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Grades.AnyAsync(g => g.Id == id);
    }
}

