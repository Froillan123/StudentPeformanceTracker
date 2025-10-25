using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface ITeacherSubjectRepository
    {
        Task<IEnumerable<TeacherSubject>> GetAllAsync();
        Task<TeacherSubject?> GetByIdAsync(int id);
        Task<IEnumerable<TeacherSubject>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<TeacherSubject>> GetBySectionSubjectIdAsync(int sectionSubjectId);
        Task<TeacherSubject?> GetByTeacherAndSectionSubjectAsync(int teacherId, int sectionSubjectId);
        Task<TeacherSubject> CreateAsync(TeacherSubject teacherSubject);
        Task<TeacherSubject> UpdateAsync(TeacherSubject teacherSubject);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByTeacherAndSectionSubjectAsync(int teacherId, int sectionSubjectId);
    }
}

