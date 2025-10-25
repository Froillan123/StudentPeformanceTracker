using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface ISectionSubjectRepository
    {
        Task<IEnumerable<SectionSubject>> GetAllAsync();
        Task<SectionSubject?> GetByIdAsync(int id);
        Task<SectionSubject?> GetByEdpCodeAsync(string edpCode);
        Task<IEnumerable<SectionSubject>> GetBySectionIdAsync(int sectionId);
        Task<IEnumerable<SectionSubject>> GetByTeacherIdAsync(int teacherId);
        Task<SectionSubject> CreateAsync(SectionSubject sectionSubject);
        Task<SectionSubject> UpdateAsync(SectionSubject sectionSubject);
        Task<bool> DeleteAsync(int id);
    }
}
