using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface ISectionRepository
    {
        Task<IEnumerable<Section>> GetAllAsync();
        Task<Section?> GetByIdAsync(int id);
        Task<IEnumerable<Section>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Section>> GetByCourseYearSemesterAsync(int courseId, int yearLevelId, int semesterId);
        Task<Section?> GetBySectionNameCourseYearSemesterAsync(string sectionName, int courseId, int yearLevelId, int semesterId);
        Task<Section> CreateAsync(Section section);
        Task<Section> UpdateAsync(Section section);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<Section> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    }
}
