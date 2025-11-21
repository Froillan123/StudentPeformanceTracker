using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class SectionRepository : ISectionRepository
    {
        private readonly AppDbContext _context;

        public SectionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Section>> GetAllAsync()
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.YearLevel)
                .Include(s => s.Semester)
                .ToListAsync();
        }

        public async Task<Section?> GetByIdAsync(int id)
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.YearLevel)
                .Include(s => s.Semester)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Section>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.YearLevel)
                .Include(s => s.Semester)
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.YearLevel.LevelNumber)
                .ThenBy(s => s.Semester.SemesterName)
                .ThenBy(s => s.SectionName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Section>> GetByCourseYearSemesterAsync(int courseId, int yearLevelId, int semesterId)
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.YearLevel)
                .Include(s => s.Semester)
                .Where(s => s.CourseId == courseId && s.YearLevelId == yearLevelId && s.SemesterId == semesterId && s.IsActive)
                .OrderBy(s => s.SectionName)
                .ToListAsync();
        }

        public async Task<Section?> GetBySectionNameCourseYearSemesterAsync(string sectionName, int courseId, int yearLevelId, int semesterId)
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.YearLevel)
                .Include(s => s.Semester)
                .FirstOrDefaultAsync(s => s.SectionName == sectionName 
                    && s.CourseId == courseId 
                    && s.YearLevelId == yearLevelId 
                    && s.SemesterId == semesterId);
        }

        public async Task<Section> CreateAsync(Section section)
        {
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();

            // Reload with includes to populate navigation properties
            return await GetByIdAsync(section.Id) ?? section;
        }

        public async Task<Section> UpdateAsync(Section section)
        {
            // Get the existing entity from the database (tracked by EF)
            var existing = await _context.Sections.FindAsync(section.Id);
            if (existing == null)
                throw new InvalidOperationException($"Section with ID {section.Id} not found");

            // Update only the scalar properties (not navigation properties)
            existing.SectionName = section.SectionName;
            existing.CourseId = section.CourseId;
            existing.YearLevelId = section.YearLevelId;
            existing.SemesterId = section.SemesterId;
            existing.MaxCapacity = section.MaxCapacity;
            existing.CurrentEnrollment = section.CurrentEnrollment;
            existing.IsActive = section.IsActive;
            existing.UpdatedAt = section.UpdatedAt;

            await _context.SaveChangesAsync();

            // Reload with includes to populate navigation properties
            return await GetByIdAsync(section.Id) ?? existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return false;

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Section> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var query = _context.Sections
                .Include(s => s.Course)
                .Include(s => s.YearLevel)
                .Include(s => s.Semester)
                .AsQueryable();
            
            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderBy(s => s.YearLevel.LevelNumber)
                .ThenBy(s => s.Semester.SemesterName)
                .ThenBy(s => s.SectionName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (items, totalCount);
        }
    }
}
