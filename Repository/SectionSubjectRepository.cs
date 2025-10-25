using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class SectionSubjectRepository : ISectionSubjectRepository
    {
        private readonly AppDbContext _context;

        public SectionSubjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SectionSubject>> GetAllAsync()
        {
            return await _context.SectionSubjects
                .Include(ss => ss.Section)
                .Include(ss => ss.Subject)
                .Include(ss => ss.Teacher)
                .ToListAsync();
        }

        public async Task<SectionSubject?> GetByIdAsync(int id)
        {
            return await _context.SectionSubjects
                .Include(ss => ss.Section)
                .Include(ss => ss.Subject)
                .Include(ss => ss.Teacher)
                .FirstOrDefaultAsync(ss => ss.Id == id);
        }

        public async Task<SectionSubject?> GetByEdpCodeAsync(string edpCode)
        {
            return await _context.SectionSubjects
                .Include(ss => ss.Section)
                .Include(ss => ss.Subject)
                .Include(ss => ss.Teacher)
                .FirstOrDefaultAsync(ss => ss.EdpCode == edpCode);
        }

        public async Task<IEnumerable<SectionSubject>> GetBySectionIdAsync(int sectionId)
        {
            return await _context.SectionSubjects
                .Include(ss => ss.Section)
                .Include(ss => ss.Subject)
                .Include(ss => ss.Teacher)
                .Where(ss => ss.SectionId == sectionId)
                .OrderBy(ss => ss.Subject.SubjectName)
                .ToListAsync();
        }

        public async Task<IEnumerable<SectionSubject>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.SectionSubjects
                .Include(ss => ss.Section)
                .Include(ss => ss.Subject)
                .Include(ss => ss.Teacher)
                .Where(ss => ss.TeacherId == teacherId)
                .OrderBy(ss => ss.Section.SectionName)
                .ThenBy(ss => ss.Subject.SubjectName)
                .ToListAsync();
        }

        public async Task<SectionSubject> CreateAsync(SectionSubject sectionSubject)
        {
            _context.SectionSubjects.Add(sectionSubject);
            await _context.SaveChangesAsync();
            return sectionSubject;
        }

        public async Task<SectionSubject> UpdateAsync(SectionSubject sectionSubject)
        {
            _context.SectionSubjects.Update(sectionSubject);
            await _context.SaveChangesAsync();
            return sectionSubject;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sectionSubject = await _context.SectionSubjects.FindAsync(id);
            if (sectionSubject == null) return false;

            _context.SectionSubjects.Remove(sectionSubject);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
