using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;
using StudentPeformanceTracker.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teacher")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly AuthService _authService;

    public TeacherController(ITeacherRepository teacherRepository, IDepartmentRepository departmentRepository, IUserRepository userRepository, AuthService authService)
    {
        _teacherRepository = teacherRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _authService = authService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTeachers()
    {
        try
        {
            var teachers = await _teacherRepository.GetAllAsync();
            var result = teachers.Select(t => new
            {
                t.Id,
                t.UserId,
                t.Email,
                t.FirstName,
                t.LastName,
                t.Phone,
                t.HighestQualification,
                t.Status,
                t.EmergencyContact,
                t.EmergencyPhone,
                Departments = t.TeacherDepartments.Where(td => td.Department != null).Select(td => new
                {
                    td.DepartmentId,
                    DepartmentName = td.Department!.DepartmentName
                }).ToList(),
                t.HireDate,
                t.CreatedAt,
                t.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teachers", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTeacher(int id)
    {
        try
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var result = new
            {
                teacher.Id,
                teacher.UserId,
                teacher.Email,
                teacher.FirstName,
                teacher.LastName,
                teacher.Phone,
                teacher.HighestQualification,
                teacher.Status,
                teacher.EmergencyContact,
                teacher.EmergencyPhone,
                Departments = teacher.TeacherDepartments.Where(td => td.Department != null).Select(td => new
                {
                    td.DepartmentId,
                    DepartmentName = td.Department!.DepartmentName
                }).ToList(),
                teacher.HireDate,
                teacher.CreatedAt,
                teacher.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teacher", error = ex.Message });
        }
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateTeacher(int id, [FromBody] UpdateTeacherRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTeacher = await _teacherRepository.GetByIdAsync(id);
            if (existingTeacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Check if email already exists (excluding current teacher)
            if (request.Email != existingTeacher.Email &&
                await _teacherRepository.EmailExistsAsync(request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Note: Department assignment is managed separately via TeacherDepartments API

            existingTeacher.Email = request.Email;
            existingTeacher.FirstName = request.FirstName;
            existingTeacher.LastName = request.LastName;
            existingTeacher.Phone = request.Phone;
            existingTeacher.HireDate = request.HireDate;

            var updatedTeacher = await _teacherRepository.UpdateAsync(existingTeacher);

            var result = new
            {
                updatedTeacher.Id,
                updatedTeacher.UserId,
                updatedTeacher.Email,
                updatedTeacher.FirstName,
                updatedTeacher.LastName,
                updatedTeacher.Phone,
                Departments = updatedTeacher.TeacherDepartments.Select(td => new
                {
                    td.DepartmentId,
                    DepartmentName = td.Department.DepartmentName
                }).ToList(),
                updatedTeacher.HireDate,
                updatedTeacher.CreatedAt,
                updatedTeacher.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating teacher", error = ex.Message });
        }
    }

    [HttpGet("profile")]
    public async Task<ActionResult<object>> GetTeacherProfile()
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Get teacher data
            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher profile not found" });
            }

            // Get user data
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User data not found" });
            }

            var result = new
            {
                // Personal Information
                Id = teacher.Id,
                UserId = teacher.UserId,
                Username = user.Username,
                Email = teacher.Email,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Phone = teacher.Phone,
                CreatedAt = teacher.CreatedAt,
                UpdatedAt = teacher.UpdatedAt,
                LastLogin = user.UpdatedAt, // Using UpdatedAt as proxy for last login
                
                // Professional Information
                HighestQualification = teacher.HighestQualification,
                Status = teacher.Status,
                EmergencyContact = teacher.EmergencyContact,
                EmergencyPhone = teacher.EmergencyPhone,
                HireDate = teacher.HireDate,
                
                // Departments
                Departments = teacher.TeacherDepartments?.Where(td => td.Department != null).Select(td => new
                {
                    td.DepartmentId,
                    DepartmentName = td.Department!.DepartmentName
                }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teacher profile", error = ex.Message });
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<object>> UpdateTeacherProfile([FromBody] UpdateTeacherProfileRequest request)
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Get teacher data
            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher profile not found" });
            }

            // Update teacher information
            teacher.FirstName = request.FirstName;
            teacher.LastName = request.LastName;
            teacher.Phone = request.Phone;
            teacher.HighestQualification = request.HighestQualification;
            teacher.EmergencyContact = request.EmergencyContact;
            teacher.EmergencyPhone = request.EmergencyPhone;
            teacher.UpdatedAt = DateTime.UtcNow;

            var updatedTeacher = await _teacherRepository.UpdateAsync(teacher);

            var result = new
            {
                Id = updatedTeacher.Id,
                UserId = updatedTeacher.UserId,
                Email = updatedTeacher.Email,
                FirstName = updatedTeacher.FirstName,
                LastName = updatedTeacher.LastName,
                Phone = updatedTeacher.Phone,
                HighestQualification = updatedTeacher.HighestQualification,
                EmergencyContact = updatedTeacher.EmergencyContact,
                EmergencyPhone = updatedTeacher.EmergencyPhone,
                UpdatedAt = updatedTeacher.UpdatedAt,
                Message = "Profile updated successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating teacher profile", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTeacher(int id)
    {
        try
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Delete from Teacher table first
            var deleted = await _teacherRepository.DeleteAsync(id);
            if (!deleted)
            {
                return StatusCode(500, new { message = "Failed to delete teacher" });
            }

            // Also delete from Users table
            try
            {
                var userDeleted = await _userRepository.DeleteAsync(teacher.UserId);
                if (!userDeleted)
                {
                    // Log warning but don't fail the operation since teacher is already deleted
                    // This could happen if the user was already deleted or doesn't exist
                }
            }
            catch (Exception)
            {
                // Log the error but don't fail the operation since teacher is already deleted
                // This could happen due to foreign key constraints
                // You might want to log this for debugging
            }

            return Ok(new { message = "Teacher and associated user account deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting teacher", error = ex.Message });
        }
    }

    [HttpGet("by-department/{departmentId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetTeachersByDepartment(int departmentId)
    {
        try
        {
            var teachers = await _teacherRepository.GetByDepartmentIdAsync(departmentId);
            var result = teachers.Select(t => new
            {
                t.Id,
                t.UserId,
                t.Email,
                t.FirstName,
                t.LastName,
                t.Phone,
                Departments = t.TeacherDepartments.Select(td => new
                {
                    td.DepartmentId,
                    DepartmentName = td.Department.DepartmentName
                }).ToList(),
                t.HireDate,
                t.CreatedAt,
                t.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teachers by department", error = ex.Message });
        }
    }

    [HttpPost("admin-create")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> AdminCreateTeacher([FromBody] AdminCreateTeacherRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if username already exists
            if (await _authService.UsernameExistsAsync(request.Username))
            {
                return Conflict(new { message = "Username already exists" });
            }

            // Check if email already exists
            if (await _authService.EmailExistsAsync(request.Email))
            {
                return Conflict(new { message = "Email already exists" });
            }

            // Create teacher registration request
            var teacherRequest = new TeacherRegisterRequest
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                HighestQualification = request.HighestQualification,
                Status = request.Status,
                EmergencyContact = request.EmergencyContact,
                EmergencyPhone = request.EmergencyPhone,
                HireDate = request.HireDate
            };

            // Register teacher with Active status (admin-created)
            var result = await _authService.RegisterTeacherAsync(teacherRequest, isAdminCreated: true);
            
            if (result == null)
            {
                return BadRequest(new { message = "Failed to create teacher account" });
            }

            return CreatedAtAction(nameof(GetTeacher), new { id = result.UserId }, new
            {
                UserId = result.UserId,
                Username = result.Username,
                Email = result.Email,
                FirstName = result.FirstName,
                LastName = result.LastName,
                Role = result.Role,
                Message = result.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating teacher", error = ex.Message });
        }
    }
}

public class UpdateTeacherProfileRequest
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? HighestQualification { get; set; }

    [MaxLength(100)]
    public string? EmergencyContact { get; set; }

    [MaxLength(20)]
    public string? EmergencyPhone { get; set; }
}

public class CreateTeacherRequest
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? HireDate { get; set; }
    // Note: Departments are assigned separately via TeacherDepartments API
}

public class UpdateTeacherRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? HighestQualification { get; set; }
    public string Status { get; set; } = "Full-time";
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateTime? HireDate { get; set; }
    // Note: Departments are managed separately via TeacherDepartments API
}
