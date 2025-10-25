using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface IYearLevelRepository
    {
        Task<IEnumerable<YearLevel>> GetAllAsync();
        Task<YearLevel?> GetByIdAsync(int id);
        Task<YearLevel?> GetByLevelNumberAsync(int levelNumber);
        Task<YearLevel> CreateAsync(YearLevel yearLevel);
        Task<YearLevel> UpdateAsync(YearLevel yearLevel);
        Task<bool> DeleteAsync(int id);
    }
}
