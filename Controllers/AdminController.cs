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
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminRepository _adminRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICourseRepository _courseRepository;

    public AdminController(
        IAdminRepository adminRepository,
        IUserRepository userRepository,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IDepartmentRepository departmentRepository,
        ICourseRepository courseRepository)
    {
        _adminRepository = adminRepository;
        _userRepository = userRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _departmentRepository = departmentRepository;
        _courseRepository = courseRepository;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<object>> GetAdminProfile()
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Get admin data
            var admin = await _adminRepository.GetByUserIdAsync(userId);
            if (admin == null)
            {
                return NotFound(new { message = "Admin profile not found" });
            }

            // Get user data
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User data not found" });
            }

            // Get system statistics
            var totalUsers = await _userRepository.GetCountAsync();
            var totalTeachers = await _teacherRepository.GetCountAsync();
            var totalStudents = await _studentRepository.GetCountAsync();
            var totalDepartments = await _departmentRepository.GetCountAsync();
            var activeUsers = await _userRepository.GetByStatusAsync("Active");
            var activeUserCount = activeUsers.Count();

            var result = new
            {
                // Personal Information
                Id = admin.Id,
                UserId = admin.UserId,
                Username = user.Username,
                Email = admin.Email,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Phone = admin.Phone,
                CreatedAt = admin.CreatedAt,
                UpdatedAt = admin.UpdatedAt,
                LastLogin = user.UpdatedAt, // Using UpdatedAt as proxy for last login
                
                // System Statistics
                Statistics = new
                {
                    TotalUsers = totalUsers,
                    TotalTeachers = totalTeachers,
                    TotalStudents = totalStudents,
                    TotalDepartments = totalDepartments,
                    ActiveUsers = activeUserCount,
                    InactiveUsers = totalUsers - activeUserCount
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving admin profile", error = ex.Message });
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<object>> UpdateAdminProfile([FromBody] UpdateAdminProfileRequest request)
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Get admin data
            var admin = await _adminRepository.GetByUserIdAsync(userId);
            if (admin == null)
            {
                return NotFound(new { message = "Admin profile not found" });
            }

            // Update admin information
            admin.FirstName = request.FirstName;
            admin.LastName = request.LastName;
            admin.Phone = request.Phone;
            admin.UpdatedAt = DateTime.UtcNow;

            var updatedAdmin = await _adminRepository.UpdateAsync(admin);

            var result = new
            {
                Id = updatedAdmin.Id,
                UserId = updatedAdmin.UserId,
                Email = updatedAdmin.Email,
                FirstName = updatedAdmin.FirstName,
                LastName = updatedAdmin.LastName,
                Phone = updatedAdmin.Phone,
                UpdatedAt = updatedAdmin.UpdatedAt,
                Message = "Profile updated successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating admin profile", error = ex.Message });
        }
    }

    [HttpGet("dashboard-stats")]
    public async Task<ActionResult<object>> GetDashboardStats()
    {
        try
        {
            // Get current user ID from JWT token to verify admin access
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Verify admin exists
            var admin = await _adminRepository.GetByUserIdAsync(userId);
            if (admin == null)
            {
                return NotFound(new { message = "Admin profile not found" });
            }

            // Get statistics from database
            var totalStudents = await _studentRepository.GetCountAsync();
            var totalTeachers = await _teacherRepository.GetCountAsync();
            var activeCourses = await _courseRepository.GetCountAsync();

            // Count inactive users as pending issues (users awaiting approval)
            var allUsers = await _userRepository.GetAllAsync();
            var pendingIssues = allUsers.Count(u => u.Status != "Active");

            var result = new
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                ActiveCourses = activeCourses,
                PendingIssues = pendingIssues
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving dashboard statistics", error = ex.Message });
        }
    }
}

public class UpdateAdminProfileRequest
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
