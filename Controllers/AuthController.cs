using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login with username/student_id and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrStudentId) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username/Student ID and password are required" });
        }

        var result = await _authService.LoginAsync(request.UsernameOrStudentId, request.Password);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        _logger.LogInformation($"User {result.Username} logged in successfully");

        return Ok(new
        {
            message = "Login successful",
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = new
            {
                userId = result.UserId,
                username = result.Username,
                studentId = result.StudentId,
                role = result.Role
            }
        });
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StudentId) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Student ID, username, and password are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters long" });
        }

        var result = await _authService.RegisterAsync(
            request.StudentId,
            request.Username,
            request.Password,
            request.Role ?? "Student"
        );

        if (result == null)
        {
            return Conflict(new { message = "User with this username or student ID already exists" });
        }

        _logger.LogInformation($"New user registered: {result.Username}");

        return CreatedAtAction(nameof(Register), new
        {
            message = result.Message,
            user = new
            {
                userId = result.UserId,
                username = result.Username,
                studentId = result.StudentId,
                role = result.Role
            }
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        var result = await _authService.RefreshAccessTokenAsync(request.RefreshToken);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        _logger.LogInformation($"Access token refreshed for user {result.Username}");

        return Ok(new
        {
            message = "Token refreshed successfully",
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = new
            {
                userId = result.UserId,
                username = result.Username,
                studentId = result.StudentId,
                role = result.Role
            }
        });
    }

    /// <summary>
    /// Logout (revoke refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        await _authService.LogoutAsync(userId);

        _logger.LogInformation($"User {userId} logged out");

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user info (protected endpoint example)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var studentId = User.FindFirst("student_id")?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            userId,
            username,
            studentId,
            role
        });
    }
}

// Request DTOs
public class LoginRequest
{
    public string UsernameOrStudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Role { get; set; }
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
