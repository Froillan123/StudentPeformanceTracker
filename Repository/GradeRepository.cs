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

    private static DateTime GetManilaTime()
    {
        // Get current UTC time and ensure it's marked as UTC for PostgreSQL
        // PostgreSQL requires DateTime with Kind=UTC for timestamp with time zone columns
        // The time stored is UTC, which represents the current moment in Manila timezone
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    private static string GenerateRemarksFromGrade(decimal gradePoint)
    {
        return gradePoint switch
        {
            >= 1.0m and <= 1.5m => "Excellent",
            > 1.5m and <= 2.0m => "Very Good",
            > 2.0m and <= 2.5m => "Good",
            > 2.5m and <= 3.0m => "Pass",
            > 3.0m and <= 5.0m => "Failed",
            _ => "Invalid Grade"
        };
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
        // Validate AssessmentType is only Midterm or Final Grade
        if (grade.AssessmentType != "Midterm" && grade.AssessmentType != "Final Grade")
        {
            throw new InvalidOperationException("AssessmentType must be either 'Midterm' or 'Final Grade'");
        }

        // Check if a grade of this type already exists for this StudentSubject
        var existingGrade = await _context.Grades
            .FirstOrDefaultAsync(g => g.StudentSubjectId == grade.StudentSubjectId 
                && g.AssessmentType == grade.AssessmentType);
        
        if (existingGrade != null)
        {
            throw new InvalidOperationException($"A {grade.AssessmentType} grade already exists for this student in this class. Please update the existing grade instead.");
        }

        // Auto-fill AssessmentName if not provided
        if (string.IsNullOrWhiteSpace(grade.AssessmentName))
        {
            grade.AssessmentName = grade.AssessmentType == "Midterm" ? "Midterm Grade" : "Final Grade";
        }

        // Auto-generate remarks based on grade point
        grade.Remarks = GenerateRemarksFromGrade(grade.GradePoint);

        // Auto-set DateGiven to Manila timezone
        grade.DateGiven = GetManilaTime();

        grade.CreatedAt = DateTime.UtcNow;
        grade.UpdatedAt = DateTime.UtcNow;

        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task<Grade> UpdateAsync(Grade grade)
    {
        // Validate AssessmentType is only Midterm or Final Grade
        if (grade.AssessmentType != "Midterm" && grade.AssessmentType != "Final Grade")
        {
            throw new InvalidOperationException("AssessmentType must be either 'Midterm' or 'Final Grade'");
        }

        // Check if another grade of this type exists for this StudentSubject (excluding current grade)
        var existingGrade = await _context.Grades
            .FirstOrDefaultAsync(g => g.StudentSubjectId == grade.StudentSubjectId 
                && g.AssessmentType == grade.AssessmentType
                && g.Id != grade.Id);
        
        if (existingGrade != null)
        {
            throw new InvalidOperationException($"A {grade.AssessmentType} grade already exists for this student in this class.");
        }

        // Auto-fill AssessmentName if not provided
        if (string.IsNullOrWhiteSpace(grade.AssessmentName))
        {
            grade.AssessmentName = grade.AssessmentType == "Midterm" ? "Midterm Grade" : "Final Grade";
        }

        // Auto-generate remarks based on grade point
        grade.Remarks = GenerateRemarksFromGrade(grade.GradePoint);

        // Auto-set DateGiven to Manila timezone
        grade.DateGiven = GetManilaTime();

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

