using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers
{
    [ApiController]
    [Route("api/v1/studentsubject")]
    public class StudentSubjectController : ControllerBase
    {
        private readonly IStudentSubjectRepository _studentSubjectRepository;
        private readonly ISectionSubjectRepository _sectionSubjectRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public StudentSubjectController(
            IStudentSubjectRepository studentSubjectRepository,
            ISectionSubjectRepository sectionSubjectRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _studentSubjectRepository = studentSubjectRepository;
            _sectionSubjectRepository = sectionSubjectRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        /// <summary>
        /// Get students enrolled in a specific section subject
        /// </summary>
        [HttpGet("sectionsubject/{sectionSubjectId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentsBySectionSubject(int sectionSubjectId)
        {
            try
            {
                var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(sectionSubjectId);
                var result = studentSubjects.Select(ss => new
                {
                    ss.Id, // StudentSubject ID
                    StudentSubjectId = ss.StudentId,
                    StudentName = $"{ss.Student?.FirstName} {ss.Student?.LastName}",
                    FirstName = ss.Student?.FirstName,
                    LastName = ss.Student?.LastName,
                    StudentId = ss.Student?.StudentId, // Actual Student's StudentId
                    Email = ss.Student?.Email,
                    YearLevel = ss.Student?.YearLevel,
                    CourseName = ss.Student?.Course?.CourseName,
                    SubjectName = ss.SectionSubject?.Subject?.SubjectName,
                    SectionName = ss.SectionSubject?.Section?.SectionName,
                    ss.Status,
                    ss.Grade,
                    ss.CreatedAt,
                    ss.UpdatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving students for section subject", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all enrolled classes for a student
        /// </summary>
        [HttpGet("student/{studentId}/enrolled")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentEnrolledClasses(int studentId)
        {
            try
            {
                var studentSubjects = await _studentSubjectRepository.GetByStudentIdAsync(studentId);
                var result = studentSubjects.Select(ss => new
                {
                    ss.Id,
                    ss.StudentId,
                    ss.SectionSubjectId,
                    ss.EnrollmentId,
                    SubjectName = ss.SectionSubject?.Subject?.SubjectName,
                    SubjectDescription = ss.SectionSubject?.Subject?.Description,
                    SectionName = ss.SectionSubject?.Section?.SectionName,
                    TeacherName = ss.SectionSubject?.Teacher != null 
                        ? $"{ss.SectionSubject.Teacher.FirstName} {ss.SectionSubject.Teacher.LastName}" 
                        : "TBA",
                    Schedule = !string.IsNullOrEmpty(ss.SectionSubject?.ScheduleDay) && !string.IsNullOrEmpty(ss.SectionSubject?.ScheduleTime)
                        ? $"{ss.SectionSubject.ScheduleDay} {ss.SectionSubject.ScheduleTime}"
                        : "TBA",
                    ScheduleDay = ss.SectionSubject?.ScheduleDay,
                    ScheduleTime = ss.SectionSubject?.ScheduleTime,
                    Room = ss.SectionSubject?.Room,
                    EdpCode = ss.SectionSubject?.EdpCode,
                    ss.Status,
                    ss.Grade,
                    ss.CreatedAt,
                    ss.UpdatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving student enrolled classes", error = ex.Message });
            }
        }

        /// <summary>
        /// Enroll student in a class using EDP code
        /// </summary>
        [HttpPost("enroll")]
        public async Task<ActionResult<object>> EnrollStudent([FromBody] EnrollStudentRequest request)
        {
            try
            {
                // Find SectionSubject by EDP code
                var sectionSubject = await _sectionSubjectRepository.GetByEdpCodeAsync(request.EdpCode);
                if (sectionSubject == null)
                {
                    return NotFound(new { message = "Invalid EDP code. Class not found." });
                }

                // Check if student already enrolled
                var existingEnrollment = await _studentSubjectRepository.GetByStudentAndSectionSubjectAsync(
                    request.StudentId, sectionSubject.Id);
                if (existingEnrollment != null)
                {
                    return BadRequest(new { message = "You are already enrolled in this class." });
                }

                // Get student's active enrollment
                var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.StudentId);
                var activeEnrollment = enrollments.FirstOrDefault(e => e.Status == "Active");
                
                if (activeEnrollment == null)
                {
                    return BadRequest(new { message = "No active enrollment found. Please contact the admin." });
                }

                // Load section details for validation
                if (sectionSubject.Section == null)
                {
                    return BadRequest(new { message = "Section information not available." });
                }

                // Validate enrollment matches section requirements
                if (activeEnrollment.CourseId != sectionSubject.Section.CourseId)
                {
                    return BadRequest(new { message = "This class is not for your course." });
                }

                if (activeEnrollment.YearLevelId != sectionSubject.Section.YearLevelId)
                {
                    return BadRequest(new { message = "This class is not for your year level." });
                }

                if (activeEnrollment.SemesterId != sectionSubject.Section.SemesterId)
                {
                    return BadRequest(new { message = "This class is not for your current semester." });
                }

                // Check capacity
                if (sectionSubject.CurrentEnrollment >= sectionSubject.MaxStudents)
                {
                    return BadRequest(new { message = "This class is already full." });
                }

                // Create StudentSubject record
                var studentSubject = new StudentSubject
                {
                    StudentId = request.StudentId,
                    SectionSubjectId = sectionSubject.Id,
                    EnrollmentId = activeEnrollment.Id,
                    Status = "Enrolled",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _studentSubjectRepository.CreateAsync(studentSubject);

                // Increment enrollment count
                sectionSubject.CurrentEnrollment++;
                sectionSubject.UpdatedAt = DateTime.UtcNow;
                await _sectionSubjectRepository.UpdateAsync(sectionSubject);

                // Reload with navigation properties
                var enrolled = await _studentSubjectRepository.GetByIdAsync(created.Id);

                var result = new
                {
                    enrolled!.Id,
                    enrolled.StudentId,
                    enrolled.SectionSubjectId,
                    enrolled.EnrollmentId,
                    SubjectName = enrolled.SectionSubject?.Subject?.SubjectName,
                    SectionName = enrolled.SectionSubject?.Section?.SectionName,
                    TeacherName = enrolled.SectionSubject?.Teacher != null
                        ? $"{enrolled.SectionSubject.Teacher.FirstName} {enrolled.SectionSubject.Teacher.LastName}"
                        : "TBA",
                    Schedule = !string.IsNullOrEmpty(enrolled.SectionSubject?.ScheduleDay) && !string.IsNullOrEmpty(enrolled.SectionSubject?.ScheduleTime)
                        ? $"{enrolled.SectionSubject.ScheduleDay} {enrolled.SectionSubject.ScheduleTime}"
                        : "TBA",
                    Room = enrolled.SectionSubject?.Room,
                    EdpCode = enrolled.SectionSubject?.EdpCode,
                    enrolled.Status,
                    enrolled.CreatedAt,
                    Message = "Successfully enrolled in class!"
                };

                return CreatedAtAction(nameof(GetStudentEnrolledClasses), new { studentId = request.StudentId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error enrolling student", error = ex.Message });
            }
        }

        /// <summary>
        /// Admin assign student to multiple classes
        /// </summary>
        [HttpPost("admin-assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<object>> AdminAssignStudentToClasses([FromBody] AdminAssignRequest request)
        {
            try
            {
                // Validate enrollment exists and is active
                var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId);
                if (enrollment == null)
                {
                    return NotFound(new { message = "Enrollment not found." });
                }

                if (enrollment.Status != "Active")
                {
                    return BadRequest(new { message = "Enrollment is not active." });
                }

                var assignedClasses = new List<object>();
                var errors = new List<string>();

                foreach (var sectionSubjectId in request.SectionSubjectIds)
                {
                    try
                    {
                        // Get section subject
                        var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(sectionSubjectId);
                        if (sectionSubject == null)
                        {
                            errors.Add($"Section subject {sectionSubjectId} not found.");
                            continue;
                        }

                        // Check if student already enrolled
                        var existingEnrollment = await _studentSubjectRepository.GetByStudentAndSectionSubjectAsync(
                            request.StudentId, sectionSubjectId);
                        if (existingEnrollment != null)
                        {
                            errors.Add($"Student already enrolled in {sectionSubject.Subject?.SubjectName}.");
                            continue;
                        }

                        // Validate enrollment matches section requirements
                        if (enrollment.CourseId != sectionSubject.Section?.CourseId)
                        {
                            errors.Add($"Class {sectionSubject.Subject?.SubjectName} is not for the student's course.");
                            continue;
                        }

                        if (enrollment.YearLevelId != sectionSubject.Section?.YearLevelId)
                        {
                            errors.Add($"Class {sectionSubject.Subject?.SubjectName} is not for the student's year level.");
                            continue;
                        }

                        if (enrollment.SemesterId != sectionSubject.Section?.SemesterId)
                        {
                            errors.Add($"Class {sectionSubject.Subject?.SubjectName} is not for the student's semester.");
                            continue;
                        }

                        // Check capacity
                        if (sectionSubject.CurrentEnrollment >= sectionSubject.MaxStudents)
                        {
                            errors.Add($"Class {sectionSubject.Subject?.SubjectName} is already full.");
                            continue;
                        }

                        // Create StudentSubject record
                        var studentSubject = new StudentSubject
                        {
                            StudentId = request.StudentId,
                            SectionSubjectId = sectionSubjectId,
                            EnrollmentId = request.EnrollmentId,
                            Status = "Enrolled",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        var created = await _studentSubjectRepository.CreateAsync(studentSubject);

                        // Increment enrollment count
                        sectionSubject.CurrentEnrollment++;
                        sectionSubject.UpdatedAt = DateTime.UtcNow;
                        await _sectionSubjectRepository.UpdateAsync(sectionSubject);

                        // Reload with navigation properties
                        var enrolled = await _studentSubjectRepository.GetByIdAsync(created.Id);

                        assignedClasses.Add(new
                        {
                            enrolled!.Id,
                            enrolled.StudentId,
                            enrolled.SectionSubjectId,
                            enrolled.EnrollmentId,
                            SubjectName = enrolled.SectionSubject?.Subject?.SubjectName,
                            SectionName = enrolled.SectionSubject?.Section?.SectionName,
                            TeacherName = enrolled.SectionSubject?.Teacher != null
                                ? $"{enrolled.SectionSubject.Teacher.FirstName} {enrolled.SectionSubject.Teacher.LastName}"
                                : "TBA",
                            Schedule = !string.IsNullOrEmpty(enrolled.SectionSubject?.ScheduleDay) && !string.IsNullOrEmpty(enrolled.SectionSubject?.ScheduleTime)
                                ? $"{enrolled.SectionSubject.ScheduleDay} {enrolled.SectionSubject.ScheduleTime}"
                                : "TBA",
                            Room = enrolled.SectionSubject?.Room,
                            EdpCode = enrolled.SectionSubject?.EdpCode,
                            enrolled.Status,
                            enrolled.CreatedAt
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error assigning to section subject {sectionSubjectId}: {ex.Message}");
                    }
                }

                var result = new
                {
                    SuccessfullyAssigned = assignedClasses.Count,
                    TotalRequested = request.SectionSubjectIds.Count,
                    AssignedClasses = assignedClasses,
                    Errors = errors
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error assigning student to classes", error = ex.Message });
            }
        }

        /// <summary>
        /// Drop/unenroll from a class
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> UnenrollStudent(int id)
        {
            try
            {
                var studentSubject = await _studentSubjectRepository.GetByIdAsync(id);
                if (studentSubject == null)
                {
                    return NotFound(new { message = "Enrollment not found." });
                }

                // Get section subject to decrement count
                var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(studentSubject.SectionSubjectId);
                if (sectionSubject != null)
                {
                    sectionSubject.CurrentEnrollment = Math.Max(0, sectionSubject.CurrentEnrollment - 1);
                    sectionSubject.UpdatedAt = DateTime.UtcNow;
                    await _sectionSubjectRepository.UpdateAsync(sectionSubject);
                }

                // Delete enrollment
                await _studentSubjectRepository.DeleteAsync(id);

                return Ok(new { message = "Successfully dropped from class." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error dropping class", error = ex.Message });
            }
        }
    }

    public class EnrollStudentRequest
    {
        public int StudentId { get; set; }
        public string EdpCode { get; set; } = string.Empty;
    }

    public class AdminAssignRequest
    {
        public int StudentId { get; set; }
        public int EnrollmentId { get; set; }
        public List<int> SectionSubjectIds { get; set; } = new List<int>();
    }
}
