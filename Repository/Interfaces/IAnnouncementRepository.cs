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
        Task<IEnumerable<Announcement>> GetAllGeneralAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetByAdminIdAsync(int adminId);
        Task<Announcement> CreateAsync(Announcement announcement);
        Task<Announcement> UpdateAsync(Announcement announcement);
        Task<bool> DeleteAsync(int id);
    }
}

