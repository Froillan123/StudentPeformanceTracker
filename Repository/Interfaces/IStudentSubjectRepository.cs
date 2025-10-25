using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Repository.Interfaces
{
    public interface IStudentSubjectRepository
    {
        Task<IEnumerable<StudentSubject>> GetAllAsync();
        Task<StudentSubject?> GetByIdAsync(int id);
        Task<IEnumerable<StudentSubject>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<StudentSubject>> GetByEnrollmentIdAsync(int enrollmentId);
        Task<IEnumerable<StudentSubject>> GetBySectionSubjectIdAsync(int sectionSubjectId);
        Task<StudentSubject?> GetByStudentAndSectionSubjectAsync(int studentId, int sectionSubjectId);
        Task<StudentSubject> CreateAsync(StudentSubject studentSubject);
        Task<StudentSubject> UpdateAsync(StudentSubject studentSubject);
        Task<bool> DeleteAsync(int id);
    }
}
