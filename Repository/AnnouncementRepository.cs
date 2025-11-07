using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly AppDbContext _context;

        public AnnouncementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await _context.Announcements
                .Include(a => a.Teacher)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            return await _context.Announcements
                .Include(a => a.Teacher)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Announcement>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.Announcements
                .Include(a => a.Teacher)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .Where(a => a.TeacherId == teacherId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetActiveAsync()
        {
            return await _context.Announcements
                .Include(a => a.Teacher)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetBySectionSubjectIdsAsync(IEnumerable<int> sectionSubjectIds)
        {
            return await _context.Announcements
                .Include(a => a.Teacher)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Subject)
                .Include(a => a.SectionSubject)
                    .ThenInclude(ss => ss.Section)
                .Where(a => a.IsActive && sectionSubjectIds.Contains(a.SectionSubjectId))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement> CreateAsync(Announcement announcement)
        {
            announcement.CreatedAt = DateTime.UtcNow;
            announcement.UpdatedAt = DateTime.UtcNow;
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<Announcement> UpdateAsync(Announcement announcement)
        {
            announcement.UpdatedAt = DateTime.UtcNow;
            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

