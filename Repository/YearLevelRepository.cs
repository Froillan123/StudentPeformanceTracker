using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class YearLevelRepository : IYearLevelRepository
    {
        private readonly AppDbContext _context;

        public YearLevelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<YearLevel>> GetAllAsync()
        {
            return await _context.YearLevels
                .OrderBy(yl => yl.LevelNumber)
                .ToListAsync();
        }

        public async Task<YearLevel?> GetByIdAsync(int id)
        {
            return await _context.YearLevels
                .FirstOrDefaultAsync(yl => yl.Id == id);
        }

        public async Task<YearLevel?> GetByLevelNumberAsync(int levelNumber)
        {
            return await _context.YearLevels
                .FirstOrDefaultAsync(yl => yl.LevelNumber == levelNumber);
        }

        public async Task<YearLevel> CreateAsync(YearLevel yearLevel)
        {
            _context.YearLevels.Add(yearLevel);
            await _context.SaveChangesAsync();
            return yearLevel;
        }

        public async Task<YearLevel> UpdateAsync(YearLevel yearLevel)
        {
            _context.YearLevels.Update(yearLevel);
            await _context.SaveChangesAsync();
            return yearLevel;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var yearLevel = await _context.YearLevels.FindAsync(id);
            if (yearLevel == null) return false;

            _context.YearLevels.Remove(yearLevel);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
