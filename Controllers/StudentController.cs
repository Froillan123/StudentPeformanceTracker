using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/student")]
public class StudentController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public StudentController(IStudentRepository studentRepository, IEnrollmentRepository enrollmentRepository)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    /// <summary>
    /// Get current student's profile (Student access only)
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Policy = "StudentOnly")]
    public async Task<ActionResult<object>> GetProfile()
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Get student by user ID
            var student = await _studentRepository.GetByUserIdAsync(userId);
            if (student == null)
            {
                return NotFound(new { message = "Student profile not found" });
            }

            // Get course name - fallback to most recent active enrollment if student's course is null
            string courseName = student.Course?.CourseName ?? "N/A";
            if (courseName == "N/A")
            {
                var enrollments = await _enrollmentRepository.GetByStudentIdAsync(student.Id);
                var activeEnrollment = enrollments.FirstOrDefault(e => e.Status == "Active" || e.Status == "Pending");
                courseName = activeEnrollment?.Course?.CourseName ?? "N/A";
            }

            var result = new
            {
                student.Id,
                student.UserId,
                student.StudentId,
                student.FirstName,
                student.LastName,
                student.Email,
                student.Phone,
                student.YearLevel,
                student.EnrollmentDate,
                student.CreatedAt,
                student.UpdatedAt,
                CourseName = courseName,
                Status = student.User?.Status ?? "Unknown"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving student profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all students with their user information (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> GetStudents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var (students, totalCount) = await _studentRepository.GetPaginatedAsync(page, pageSize);
            
            // Get all student IDs to fetch enrollments in batch
            var studentIds = students.Select(s => s.Id).ToList();
            var allEnrollments = new List<Enrollment>();
            foreach (var studentId in studentIds)
            {
                var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
                allEnrollments.AddRange(enrollments);
            }
            
            var result = students.Select(s => {
                // Get course name - fallback to most recent active enrollment if student's course is null
                string courseName = s.Course?.CourseName ?? "N/A";
                if (courseName == "N/A")
                {
                    var studentEnrollments = allEnrollments.Where(e => e.StudentId == s.Id);
                    var activeEnrollment = studentEnrollments.FirstOrDefault(e => e.Status == "Active" || e.Status == "Pending");
                    courseName = activeEnrollment?.Course?.CourseName ?? "N/A";
                }
                
                return new
                {
                    s.Id,
                    s.UserId,
                    s.StudentId,
                    s.FirstName,
                    s.LastName,
                    s.Email,
                    s.Phone,
                    s.YearLevel,
                    s.EnrollmentDate,
                    s.CreatedAt,
                    s.UpdatedAt,
                    CourseName = courseName,
                    Status = s.User?.Status ?? "Unknown"
                };
            });

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var paginatedResult = new PaginatedResult<object>
            {
                Data = result,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(paginatedResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving students", error = ex.Message });
        }
    }

    /// <summary>
    /// Get student by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> GetStudent(int id)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // Get course name - fallback to most recent active enrollment if student's course is null
            string courseName = student.Course?.CourseName ?? "N/A";
            if (courseName == "N/A")
            {
                var enrollments = await _enrollmentRepository.GetByStudentIdAsync(student.Id);
                var activeEnrollment = enrollments.FirstOrDefault(e => e.Status == "Active" || e.Status == "Pending");
                courseName = activeEnrollment?.Course?.CourseName ?? "N/A";
            }

            var result = new
            {
                student.Id,
                student.UserId,
                student.StudentId,
                student.FirstName,
                student.LastName,
                student.Email,
                student.Phone,
                student.YearLevel,
                student.EnrollmentDate,
                student.CreatedAt,
                student.UpdatedAt,
                CourseName = courseName,
                Status = student.User?.Status ?? "Unknown"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving student", error = ex.Message });
        }
    }

    /// <summary>
    /// Get students by course ID (Admin only)
    /// </summary>
    [HttpGet("course/{courseId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<object>>> GetStudentsByCourse(int courseId)
    {
        try
        {
            var students = await _studentRepository.GetByCourseIdAsync(courseId);
            var result = students.Select(s => new
            {
                s.Id,
                s.UserId,
                s.StudentId,
                s.FirstName,
                s.LastName,
                s.Email,
                s.Phone,
                s.YearLevel,
                s.EnrollmentDate,
                s.CreatedAt,
                s.UpdatedAt,
                CourseName = s.Course?.CourseName ?? "N/A",
                Status = s.User?.Status ?? "Unknown"
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving students by course", error = ex.Message });
        }
    }

    /// <summary>
    /// Get students by year level (Admin only)
    /// </summary>
    [HttpGet("year/{yearLevel}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<object>>> GetStudentsByYearLevel(int yearLevel)
    {
        try
        {
            var students = await _studentRepository.GetByYearLevelAsync(yearLevel);
            var result = students.Select(s => new
            {
                s.Id,
                s.UserId,
                s.StudentId,
                s.FirstName,
                s.LastName,
                s.Email,
                s.Phone,
                s.YearLevel,
                s.EnrollmentDate,
                s.CreatedAt,
                s.UpdatedAt,
                CourseName = s.Course?.CourseName ?? "N/A",
                Status = s.User?.Status ?? "Unknown"
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving students by year level", error = ex.Message });
        }
    }

    /// <summary>
    /// Update current student's profile (Student access only)
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Policy = "StudentOnly")]
    public async Task<ActionResult<object>> UpdateProfile([FromBody] UpdateStudentProfileRequest request)
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Get student data
            var student = await _studentRepository.GetByUserIdAsync(userId);
            if (student == null)
            {
                return NotFound(new { message = "Student profile not found" });
            }

            // Update student information (only allow updating certain fields)
            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.Phone = request.Phone;
            student.UpdatedAt = DateTime.UtcNow;

            var updatedStudent = await _studentRepository.UpdateAsync(student);

            // Get course name
            string courseName = updatedStudent.Course?.CourseName ?? "N/A";
            if (courseName == "N/A")
            {
                var enrollments = await _enrollmentRepository.GetByStudentIdAsync(updatedStudent.Id);
                var activeEnrollment = enrollments.FirstOrDefault(e => e.Status == "Active" || e.Status == "Pending");
                courseName = activeEnrollment?.Course?.CourseName ?? "N/A";
            }

            var result = new
            {
                Id = updatedStudent.Id,
                UserId = updatedStudent.UserId,
                StudentId = updatedStudent.StudentId,
                FirstName = updatedStudent.FirstName,
                LastName = updatedStudent.LastName,
                Email = updatedStudent.Email,
                Phone = updatedStudent.Phone,
                YearLevel = updatedStudent.YearLevel,
                EnrollmentDate = updatedStudent.EnrollmentDate,
                CourseName = courseName,
                UpdatedAt = updatedStudent.UpdatedAt,
                Message = "Profile updated successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating student profile", error = ex.Message });
        }
    }
}

public class UpdateStudentProfileRequest
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }
}
