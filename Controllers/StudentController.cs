using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Repository.Interfaces;
using System.Security.Claims;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/student")]
public class StudentController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;

    public StudentController(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
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
                CourseName = student.Course?.CourseName ?? "N/A",
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
    public async Task<ActionResult<IEnumerable<object>>> GetStudents()
    {
        try
        {
            var students = await _studentRepository.GetAllAsync();
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
                CourseName = student.Course?.CourseName ?? "N/A",
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
}
