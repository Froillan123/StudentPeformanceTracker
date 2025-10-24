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

            if (user.Status == "Active")
            {
                return BadRequest(new { message = "User is already active" });
            }

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

            if (user.Status == "Inactive")
            {
                return BadRequest(new { message = "User is already inactive" });
            }

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

            var result = new
            {
                user.Id,
                user.Username,
                user.Role,
                user.Status,
                user.CreatedAt,
                user.UpdatedAt,
                Email = GetUserEmail(user),
                FirstName = GetUserFirstName(user),
                LastName = GetUserLastName(user),
                Phone = GetUserPhone(user)
            };

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
