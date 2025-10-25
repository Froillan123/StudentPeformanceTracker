using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class CourseSubjectRepository : ICourseSubjectRepository
    {
        private readonly AppDbContext _context;

        public CourseSubjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseSubject>> GetAllAsync()
        {
            return await _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Include(cs => cs.YearLevel)
                .Include(cs => cs.Semester)
                .ToListAsync();
        }

        public async Task<CourseSubject?> GetByIdAsync(int id)
        {
            return await _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Include(cs => cs.YearLevel)
                .Include(cs => cs.Semester)
                .FirstOrDefaultAsync(cs => cs.Id == id);
        }

        public async Task<IEnumerable<CourseSubject>> GetByCourseIdAsync(int courseId)
        {
            return await _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Include(cs => cs.YearLevel)
                .Include(cs => cs.Semester)
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.YearLevel.LevelNumber)
                .ThenBy(cs => cs.Semester.SemesterName)
                .ThenBy(cs => cs.Subject.SubjectName)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseSubject>> GetByCourseAndYearAsync(int courseId, int yearLevelId)
        {
            return await _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Include(cs => cs.YearLevel)
                .Include(cs => cs.Semester)
                .Where(cs => cs.CourseId == courseId && cs.YearLevelId == yearLevelId)
                .OrderBy(cs => cs.Semester.SemesterName)
                .ThenBy(cs => cs.Subject.SubjectName)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseSubject>> GetByCourseYearSemesterAsync(int courseId, int yearLevelId, int semesterId)
        {
            return await _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Include(cs => cs.YearLevel)
                .Include(cs => cs.Semester)
                .Where(cs => cs.CourseId == courseId && cs.YearLevelId == yearLevelId && cs.SemesterId == semesterId)
                .OrderBy(cs => cs.Subject.SubjectName)
                .ToListAsync();
        }

        public async Task<CourseSubject> CreateAsync(CourseSubject courseSubject)
        {
            _context.CourseSubjects.Add(courseSubject);
            await _context.SaveChangesAsync();
            return courseSubject;
        }

        public async Task<CourseSubject> UpdateAsync(CourseSubject courseSubject)
        {
            _context.CourseSubjects.Update(courseSubject);
            await _context.SaveChangesAsync();
            return courseSubject;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var courseSubject = await _context.CourseSubjects.FindAsync(id);
            if (courseSubject == null) return false;

            _context.CourseSubjects.Remove(courseSubject);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
