using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Repository
{
    public class StudentSubjectRepository : IStudentSubjectRepository
    {
        private readonly AppDbContext _context;

        public StudentSubjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentSubject>> GetAllAsync()
        {
            return await _context.StudentSubjects
                .Include(ss => ss.Student)
                .Include(ss => ss.SectionSubject)
                .Include(ss => ss.Enrollment)
                .ToListAsync();
        }

        public async Task<StudentSubject?> GetByIdAsync(int id)
        {
            return await _context.StudentSubjects
                .Include(ss => ss.Student)
                .Include(ss => ss.SectionSubject)
                .Include(ss => ss.Enrollment)
                .FirstOrDefaultAsync(ss => ss.Id == id);
        }

        public async Task<IEnumerable<StudentSubject>> GetByStudentIdAsync(int studentId)
        {
            return await _context.StudentSubjects
                .Include(ss => ss.Student)
                .Include(ss => ss.SectionSubject)
                .Include(ss => ss.Enrollment)
                .Where(ss => ss.StudentId == studentId)
                .OrderBy(ss => ss.SectionSubject.Subject.SubjectName)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentSubject>> GetByEnrollmentIdAsync(int enrollmentId)
        {
            return await _context.StudentSubjects
                .Include(ss => ss.Student)
                .Include(ss => ss.SectionSubject)
                .Include(ss => ss.Enrollment)
                .Where(ss => ss.EnrollmentId == enrollmentId)
                .OrderBy(ss => ss.SectionSubject.Subject.SubjectName)
                .ToListAsync();
        }

        public async Task<StudentSubject?> GetByStudentAndSectionSubjectAsync(int studentId, int sectionSubjectId)
        {
            return await _context.StudentSubjects
                .Include(ss => ss.Student)
                .Include(ss => ss.SectionSubject)
                .Include(ss => ss.Enrollment)
                .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SectionSubjectId == sectionSubjectId);
        }

        public async Task<StudentSubject> CreateAsync(StudentSubject studentSubject)
        {
            _context.StudentSubjects.Add(studentSubject);
            await _context.SaveChangesAsync();
            return studentSubject;
        }

        public async Task<StudentSubject> UpdateAsync(StudentSubject studentSubject)
        {
            _context.StudentSubjects.Update(studentSubject);
            await _context.SaveChangesAsync();
            return studentSubject;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var studentSubject = await _context.StudentSubjects.FindAsync(id);
            if (studentSubject == null) return false;

            _context.StudentSubjects.Remove(studentSubject);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
