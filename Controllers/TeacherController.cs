using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
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
    private readonly AppDbContext _context;
    private readonly ISectionSubjectRepository _sectionSubjectRepository;

    public TeacherController(ITeacherRepository teacherRepository, IDepartmentRepository departmentRepository, IUserRepository userRepository, AuthService authService, AppDbContext context, ISectionSubjectRepository sectionSubjectRepository)
    {
        _teacherRepository = teacherRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _authService = authService;
        _context = context;
        _sectionSubjectRepository = sectionSubjectRepository;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetTeachers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var (teachers, totalCount) = await _teacherRepository.GetPaginatedAsync(page, pageSize);
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

            // Trim whitespace from inputs
            if (request.Email != null)
                request.Email = request.Email.Trim();
            if (request.FirstName != null)
                request.FirstName = request.FirstName.Trim();
            if (request.LastName != null)
                request.LastName = request.LastName.Trim();
            if (request.Phone != null)
                request.Phone = request.Phone.Trim();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return BadRequest(new { message = "First name is required" });
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { message = "Last name is required" });
            }

            // Check if email already exists (excluding current teacher) - case-insensitive check
            var emailChanged = !string.Equals(request.Email, existingTeacher.Email, StringComparison.OrdinalIgnoreCase);
            if (emailChanged)
            {
                // Check case-insensitive email match across all roles
                var emailExistsCaseInsensitive = await _context.Teachers
                    .AnyAsync(t => t.Id != id && EF.Functions.ILike(t.Email, request.Email));
                
                if (emailExistsCaseInsensitive)
                {
                    return Conflict(new { 
                        message = $"The email address '{request.Email}' is already registered. Please use a different email address.",
                        field = "email"
                    });
                }

                // Also check in Students and Admins
                var emailExistsInStudents = await _context.Students
                    .AnyAsync(s => EF.Functions.ILike(s.Email, request.Email));
                if (emailExistsInStudents)
                {
                    return Conflict(new { 
                        message = $"The email address '{request.Email}' is already registered. Please use a different email address.",
                        field = "email"
                    });
                }

                var emailExistsInAdmins = await _context.Admins
                    .AnyAsync(a => EF.Functions.ILike(a.Email, request.Email));
                if (emailExistsInAdmins)
                {
                    return Conflict(new { 
                        message = $"The email address '{request.Email}' is already registered. Please use a different email address.",
                        field = "email"
                    });
                }
            }

            // Note: Department assignment is managed separately via TeacherDepartments API

            existingTeacher.Email = request.Email;
            existingTeacher.FirstName = request.FirstName;
            existingTeacher.LastName = request.LastName;
            existingTeacher.Phone = request.Phone;
            
            // Update optional fields if provided
            if (request.HighestQualification != null)
                existingTeacher.HighestQualification = request.HighestQualification;
            if (request.Status != null)
                existingTeacher.Status = request.Status;
            if (request.EmergencyContact != null)
                existingTeacher.EmergencyContact = request.EmergencyContact;
            if (request.EmergencyPhone != null)
                existingTeacher.EmergencyPhone = request.EmergencyPhone;
            if (request.HireDate.HasValue)
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
                updatedTeacher.HighestQualification,
                updatedTeacher.Status,
                updatedTeacher.EmergencyContact,
                updatedTeacher.EmergencyPhone,
                Departments = updatedTeacher.TeacherDepartments.Select(td => new
                {
                    td.DepartmentId,
                    DepartmentName = td.Department?.DepartmentName
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

            // Use execution strategy to support retry on failure with transactions
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Use a transaction to ensure all deletions succeed or fail together
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // First, delete related records that don't have cascade delete
                    // Delete TeacherSubjects
                    var teacherSubjects = await _context.TeacherSubjects
                        .Where(ts => ts.TeacherId == id)
                        .ToListAsync();
                    _context.TeacherSubjects.RemoveRange(teacherSubjects);

                    // Delete SectionSubjects where this teacher is assigned
                    var sectionSubjects = await _context.SectionSubjects
                        .Where(ss => ss.TeacherId == id)
                        .ToListAsync();
                    _context.SectionSubjects.RemoveRange(sectionSubjects);

                    // Save changes for related records
                    await _context.SaveChangesAsync();

                    // Delete from Teacher table (this will cascade to TeacherDepartments)
                    var teacherDeleted = await _teacherRepository.DeleteAsync(id);
                    if (!teacherDeleted)
                    {
                        await transaction.RollbackAsync();
                        throw new InvalidOperationException("Failed to delete teacher");
                    }

                    // Delete from Users table
                    var userDeleted = await _userRepository.DeleteAsync(teacher.UserId);
                    if (!userDeleted)
                    {
                        await transaction.RollbackAsync();
                        throw new InvalidOperationException("Failed to delete associated user account");
                    }

                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

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

            // Trim whitespace from inputs
            if (request.Username != null)
                request.Username = request.Username.Trim();
            if (request.Email != null)
                request.Email = request.Email.Trim();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { message = "Username is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Password is required" });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return BadRequest(new { message = "First name is required" });
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { message = "Last name is required" });
            }

            // Check if username already exists (case-insensitive check using EF.Functions.ILike for PostgreSQL)
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return Conflict(new { 
                    message = $"The username '{request.Username}' is already taken. Please choose a different username.",
                    field = "username"
                });
            }

            // Check case-insensitive username match using database query
            var usernameExistsCaseInsensitive = await _context.Users
                .AnyAsync(u => EF.Functions.ILike(u.Username, request.Username));
            if (usernameExistsCaseInsensitive)
            {
                return Conflict(new { 
                    message = $"The username '{request.Username}' is already taken. Please choose a different username.",
                    field = "username"
                });
            }

            // Check if email already exists
            if (await _authService.EmailExistsAsync(request.Email))
            {
                return Conflict(new { 
                    message = $"The email address '{request.Email}' is already registered. Please use a different email address.",
                    field = "email"
                });
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

    /// <summary>
    /// Get current teacher's classes (SectionSubjects)
    /// </summary>
    [HttpGet("classes")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyClasses()
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

            // Get teacher's classes
            var sectionSubjects = await _sectionSubjectRepository.GetByTeacherIdAsync(teacher.Id);
            
            var result = sectionSubjects.Select(ss => new
            {
                ss.Id,
                ss.SectionId,
                SectionName = ss.Section?.SectionName,
                ss.SubjectId,
                SubjectName = ss.Subject?.SubjectName,
                SubjectDescription = ss.Subject?.Description,
                ss.TeacherId,
                TeacherName = ss.Teacher != null ? $"{ss.Teacher.FirstName} {ss.Teacher.LastName}" : "N/A",
                Schedule = !string.IsNullOrEmpty(ss.ScheduleDay) && !string.IsNullOrEmpty(ss.ScheduleTime)
                    ? $"{ss.ScheduleDay} {ss.ScheduleTime}"
                    : "TBA",
                ss.ScheduleDay,
                ss.ScheduleTime,
                ss.Room,
                ss.EdpCode,
                ss.MaxStudents,
                ss.CurrentEnrollment,
                ss.IsActive,
                ss.CreatedAt,
                ss.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving classes", error = ex.Message });
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
