using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user-management")]
[Authorize(Policy = "AdminOnly")]
public class UserManagementController : ControllerBase
{
    private readonly UserManagementService _userManagementService;

    public UserManagementController(UserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<object>>> GetPendingUsers()
    {
        try
        {
            var users = await _userManagementService.GetPendingUsersAsync();
            var result = users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                u.Status,
                u.CreatedAt,
                Email = GetUserEmail(u),
                FirstName = GetUserFirstName(u),
                LastName = GetUserLastName(u),
                Phone = GetUserPhone(u)
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving pending users", error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<object>>> GetActiveUsers()
    {
        try
        {
            var users = await _userManagementService.GetActiveUsersAsync();
            var result = users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                u.Status,
                u.CreatedAt,
                Email = GetUserEmail(u),
                FirstName = GetUserFirstName(u),
                LastName = GetUserLastName(u),
                Phone = GetUserPhone(u)
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving active users", error = ex.Message });
        }
    }

    [HttpPut("{userId}/activate")]
    public async Task<ActionResult> ActivateUser(int userId)
    {
        try
        {
            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Allow re-activation - just update the status
            var success = await _userManagementService.ActivateUserAsync(userId);
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to activate user" });
            }

            return Ok(new { message = "User activated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error activating user", error = ex.Message });
        }
    }

    [HttpPut("{userId}/deactivate")]
    public async Task<ActionResult> DeactivateUser(int userId)
    {
        try
        {
            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Allow re-deactivation - just update the status
            var success = await _userManagementService.DeactivateUserAsync(userId);
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to deactivate user" });
            }

            return Ok(new { message = "User deactivated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deactivating user", error = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<object>> GetUser(int userId)
    {
        try
        {
            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Build base result
            var result = new Dictionary<string, object>
            {
                ["id"] = user.Id,
                ["username"] = user.Username,
                ["role"] = user.Role,
                ["status"] = user.Status,
                ["createdAt"] = user.CreatedAt,
                ["updatedAt"] = user.UpdatedAt,
                ["email"] = GetUserEmail(user),
                ["firstName"] = GetUserFirstName(user),
                ["lastName"] = GetUserLastName(user),
                ["phone"] = GetUserPhone(user) ?? "N/A"
            };

            // Add student-specific fields if user is a student
            if (user.Role == "Student" && user.Student != null)
            {
                result["studentId"] = user.Student.StudentId ?? "N/A";
                result["yearLevel"] = user.Student.YearLevel ?? 0;
                result["courseName"] = user.Student.Course?.CourseName ?? "N/A";
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
        }
    }

    private string GetUserEmail(User user)
    {
        return user.Role switch
        {
            "Student" => user.Student?.Email ?? "",
            "Teacher" => user.Teacher?.Email ?? "",
            "Admin" => user.Admin?.Email ?? "",
            _ => ""
        };
    }

    private string GetUserFirstName(User user)
    {
        return user.Role switch
        {
            "Student" => user.Student?.FirstName ?? "",
            "Teacher" => user.Teacher?.FirstName ?? "",
            "Admin" => user.Admin?.FirstName ?? "",
            _ => ""
        };
    }

    private string GetUserLastName(User user)
    {
        return user.Role switch
        {
            "Student" => user.Student?.LastName ?? "",
            "Teacher" => user.Teacher?.LastName ?? "",
            "Admin" => user.Admin?.LastName ?? "",
            _ => ""
        };
    }

    private string? GetUserPhone(User user)
    {
        return user.Role switch
        {
            "Student" => user.Student?.Phone,
            "Teacher" => user.Teacher?.Phone,
            "Admin" => user.Admin?.Phone,
            _ => null
        };
    }
}
