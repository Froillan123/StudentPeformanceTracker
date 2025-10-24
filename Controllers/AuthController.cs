using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using StudentPeformanceTracker.Services;
using StudentPeformanceTracker.DTO;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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

        // Set HttpOnly cookies for tokens (secure, JS cannot access)
        SetTokenCookies(result.AccessToken, result.RefreshToken);

        return Ok(new
        {
            message = "Login successful",
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
    /// Register a new user with role-specific information
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validate common required fields
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { message = "Username, password, email, first name, and last name are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters long" });
        }

        // Validate role-specific fields
        if (request.Role == "Student" && string.IsNullOrWhiteSpace(request.StudentNumber))
        {
            return BadRequest(new { message = "Student number is required for student registration" });
        }

        if (request.Role == "Teacher" && string.IsNullOrWhiteSpace(request.Department))
        {
            return BadRequest(new { message = "Department is required for teacher registration" });
        }

        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            return Conflict(new { message = "User with this username or email already exists" });
        }

        _logger.LogInformation($"New {request.Role.ToLower()} registered: {result.Username}");

        return CreatedAtAction(nameof(Register), new
        {
            message = result.Message,
            user = new
            {
                userId = result.UserId,
                username = result.Username,
                email = result.Email,
                firstName = result.FirstName,
                lastName = result.LastName,
                role = result.Role
            }
        });
    }

    /// <summary>
    /// Register a new student
    /// </summary>
    [HttpPost("register/student")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegisterRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.StudentNumber))
        {
            return BadRequest(new { message = "All required fields must be provided" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters long" });
        }

        if (request.CourseId <= 0)
        {
            return BadRequest(new { message = "Valid CourseId is required" });
        }

        try
        {
            var result = await _authService.RegisterStudentAsync(request);

            if (result == null)
            {
                return Conflict(new { message = "User with this username or email already exists" });
            }

            _logger.LogInformation($"New student registered: {result.Username}");

            return CreatedAtAction(nameof(RegisterStudent), new
            {
                message = result.Message,
                user = new
                {
                    userId = result.UserId,
                    username = result.Username,
                    email = result.Email,
                    firstName = result.FirstName,
                    lastName = result.LastName,
                    role = result.Role
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Register a new teacher
    /// </summary>
    [HttpPost("register/teacher")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterTeacher([FromBody] TeacherRegisterRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            request.DepartmentId <= 0)
        {
            return BadRequest(new { message = "All required fields must be provided" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters long" });
        }

        var result = await _authService.RegisterTeacherAsync(request);

        if (result == null)
        {
            return Conflict(new { message = "User with this username or email already exists" });
        }

        _logger.LogInformation($"New teacher registered: {result.Username}");

        return CreatedAtAction(nameof(RegisterTeacher), new
        {
            message = result.Message,
            user = new
            {
                userId = result.UserId,
                username = result.Username,
                email = result.Email,
                firstName = result.FirstName,
                lastName = result.LastName,
                role = result.Role
            }
        });
    }

    /// <summary>
    /// Register a new admin
    /// </summary>
    [HttpPost("register/admin")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { message = "All required fields must be provided" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters long" });
        }

        var result = await _authService.RegisterAdminAsync(request);

        if (result == null)
        {
            return Conflict(new { message = "User with this username or email already exists" });
        }

        _logger.LogInformation($"New admin registered: {result.Username}");

        return CreatedAtAction(nameof(RegisterAdmin), new
        {
            message = result.Message,
            user = new
            {
                userId = result.UserId,
                username = result.Username,
                email = result.Email,
                firstName = result.FirstName,
                lastName = result.LastName,
                role = result.Role
            }
        });
    }

    /// <summary>
    /// Refresh access token using refresh token from cookie
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        // Read refresh token from HttpOnly cookie
        var refreshToken = Request.Cookies["refresh_token"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token not found" });
        }

        var result = await _authService.RefreshAccessTokenAsync(refreshToken);

        if (result == null)
        {
            ClearTokenCookies(); // Clear invalid cookies
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        // Set new tokens in cookies
        SetTokenCookies(result.AccessToken, result.RefreshToken);

        _logger.LogInformation($"Access token refreshed for user {result.Username}");

        return Ok(new
        {
            message = "Token refreshed successfully",
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

        // Clear HttpOnly cookies
        ClearTokenCookies();

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

    // Helper methods for cookie management
    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        // Access Token Cookie (short-lived, 15 minutes)
        var accessTokenCookieOptions = new CookieOptions
        {
            HttpOnly = true, // Prevents JavaScript access (XSS protection)
            Secure = true, // Only sent over HTTPS
            SameSite = SameSiteMode.Strict, // CSRF protection
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        };
        Response.Cookies.Append("access_token", accessToken, accessTokenCookieOptions);

        // Refresh Token Cookie (long-lived, 7 days)
        var refreshTokenCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refresh_token", refreshToken, refreshTokenCookieOptions);

        _logger.LogInformation("Tokens set in HttpOnly cookies");
    }

    private void ClearTokenCookies()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        _logger.LogInformation("Token cookies cleared");
    }
}
