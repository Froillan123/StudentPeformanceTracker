using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class TeacherSubjectRepository : ITeacherSubjectRepository
    {
        private readonly AppDbContext _context;

        public TeacherSubjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeacherSubject>> GetAllAsync()
        {
            return await _context.TeacherSubjects
                .Include(ts => ts.Teacher)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .OrderBy(ts => ts.CreatedAt)
                .ToListAsync();
        }

        public async Task<TeacherSubject?> GetByIdAsync(int id)
        {
            return await _context.TeacherSubjects
                .Include(ts => ts.Teacher)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .FirstOrDefaultAsync(ts => ts.Id == id);
        }

        public async Task<IEnumerable<TeacherSubject>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.TeacherSubjects
                .Include(ts => ts.Teacher)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .Where(ts => ts.TeacherId == teacherId)
                .OrderBy(ts => ts.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeacherSubject>> GetBySectionSubjectIdAsync(int sectionSubjectId)
        {
            return await _context.TeacherSubjects
                .Include(ts => ts.Teacher)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .Where(ts => ts.SectionSubjectId == sectionSubjectId)
                .OrderBy(ts => ts.IsPrimary ? 0 : 1)
                .ThenBy(ts => ts.CreatedAt)
                .ToListAsync();
        }

        public async Task<TeacherSubject?> GetByTeacherAndSectionSubjectAsync(int teacherId, int sectionSubjectId)
        {
            return await _context.TeacherSubjects
                .Include(ts => ts.Teacher)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(ts => ts.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.SectionSubjectId == sectionSubjectId);
        }

        public async Task<TeacherSubject> CreateAsync(TeacherSubject teacherSubject)
        {
            teacherSubject.CreatedAt = DateTime.UtcNow;
            teacherSubject.UpdatedAt = DateTime.UtcNow;
            
            _context.TeacherSubjects.Add(teacherSubject);
            await _context.SaveChangesAsync();
            return teacherSubject;
        }

        public async Task<TeacherSubject> UpdateAsync(TeacherSubject teacherSubject)
        {
            teacherSubject.UpdatedAt = DateTime.UtcNow;
            
            _context.TeacherSubjects.Update(teacherSubject);
            await _context.SaveChangesAsync();
            return teacherSubject;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacherSubject = await _context.TeacherSubjects.FindAsync(id);
            if (teacherSubject == null) return false;

            _context.TeacherSubjects.Remove(teacherSubject);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByTeacherAndSectionSubjectAsync(int teacherId, int sectionSubjectId)
        {
            var teacherSubject = await _context.TeacherSubjects
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.SectionSubjectId == sectionSubjectId);
            
            if (teacherSubject == null) return false;

            _context.TeacherSubjects.Remove(teacherSubject);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

