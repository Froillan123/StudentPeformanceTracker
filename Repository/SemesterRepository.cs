using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class SemesterRepository : ISemesterRepository
    {
        private readonly AppDbContext _context;

        public SemesterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Semester>> GetAllAsync()
        {
            return await _context.Semesters
                .OrderByDescending(s => s.SchoolYear)
                .ThenBy(s => s.SemesterName)
                .ToListAsync();
        }

        public async Task<Semester?> GetByIdAsync(int id)
        {
            return await _context.Semesters
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Semester?> GetBySemesterCodeAsync(string semesterCode)
        {
            return await _context.Semesters
                .FirstOrDefaultAsync(s => s.SemesterCode == semesterCode);
        }

        public async Task<IEnumerable<Semester>> GetActiveAsync()
        {
            return await _context.Semesters
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.SchoolYear)
                .ThenBy(s => s.SemesterName)
                .ToListAsync();
        }

        public async Task<Semester> CreateAsync(Semester semester)
        {
            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();
            return semester;
        }

        public async Task<Semester> UpdateAsync(Semester semester)
        {
            _context.Semesters.Update(semester);
            await _context.SaveChangesAsync();
            return semester;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null) return false;

            _context.Semesters.Remove(semester);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
