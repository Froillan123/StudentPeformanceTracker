using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Services
{
    public class EnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentSubjectRepository _studentSubjectRepository;

        public EnrollmentService(IEnrollmentRepository enrollmentRepository, IStudentSubjectRepository studentSubjectRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentSubjectRepository = studentSubjectRepository;
        }

        public async Task<IEnumerable<EnrollmentDto>> GetAllAsync()
        {
            var enrollments = await _enrollmentRepository.GetAllAsync();
            return enrollments.Select(MapToDto);
        }

        public async Task<EnrollmentDto?> GetByIdAsync(int id)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(id);
            return enrollment != null ? MapToDto(enrollment) : null;
        }

        public async Task<IEnumerable<EnrollmentDto>> GetByStudentIdAsync(int studentId)
        {
            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
            return enrollments.Select(MapToDto);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetByCourseIdAsync(int courseId)
        {
            var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId);
            return enrollments.Select(MapToDto);
        }

        public async Task<EnrollmentDto> CreateAsync(EnrollmentCreateRequest request)
        {
            // Check if enrollment already exists
            var existingEnrollment = await _enrollmentRepository.GetByStudentCourseYearSemesterAsync(
                request.StudentId, request.CourseId, request.YearLevelId, request.SemesterId);

            if (existingEnrollment != null)
            {
                throw new InvalidOperationException("Student is already enrolled in this course for the specified year and semester.");
            }

            var enrollment = new Enrollment
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                YearLevelId = request.YearLevelId,
                SemesterId = request.SemesterId,
                EnrollmentType = request.EnrollmentType,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active"
            };

            var created = await _enrollmentRepository.CreateAsync(enrollment);
            return MapToDto(created);
        }

        public async Task<EnrollmentDto> UpdateAsync(Enrollment enrollment)
        {
            var updated = await _enrollmentRepository.UpdateAsync(enrollment);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _enrollmentRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<StudentSubject>> GetStudentSubjectsAsync(int enrollmentId)
        {
            return await _studentSubjectRepository.GetByEnrollmentIdAsync(enrollmentId);
        }

        private static EnrollmentDto MapToDto(Enrollment enrollment)
        {
            return new EnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                StudentName = $"{enrollment.Student?.FirstName} {enrollment.Student?.LastName}".Trim(),
                StudentIdNumber = enrollment.Student?.StudentId ?? string.Empty,
                CourseId = enrollment.CourseId,
                CourseName = enrollment.Course?.CourseName ?? string.Empty,
                YearLevelId = enrollment.YearLevelId,
                YearLevelName = enrollment.YearLevel?.LevelName ?? string.Empty,
                SemesterId = enrollment.SemesterId,
                SemesterName = enrollment.Semester?.SemesterName ?? string.Empty,
                SchoolYear = enrollment.Semester?.SchoolYear ?? string.Empty,
                EnrollmentType = enrollment.EnrollmentType,
                EnrollmentDate = enrollment.EnrollmentDate,
                Status = enrollment.Status,
                CreatedAt = enrollment.CreatedAt,
                UpdatedAt = enrollment.UpdatedAt
            };
        }
    }
}
