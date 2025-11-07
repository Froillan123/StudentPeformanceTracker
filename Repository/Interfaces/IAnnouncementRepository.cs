using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface IAnnouncementRepository
    {
        Task<IEnumerable<Announcement>> GetAllAsync();
        Task<Announcement?> GetByIdAsync(int id);
        Task<IEnumerable<Announcement>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<Announcement>> GetActiveAsync();
        Task<IEnumerable<Announcement>> GetBySectionSubjectIdsAsync(IEnumerable<int> sectionSubjectIds);
        Task<Announcement> CreateAsync(Announcement announcement);
        Task<Announcement> UpdateAsync(Announcement announcement);
        Task<bool> DeleteAsync(int id);
    }
}

