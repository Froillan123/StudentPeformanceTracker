using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Helpers;
using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly RedisService _redisService;

    public AuthService(AppDbContext context, JwtService jwtService, RedisService redisService)
    {
        _context = context;
        _jwtService = jwtService;
        _redisService = redisService;
    }

    public async Task<AuthResponse?> LoginAsync(string usernameOrStudentId, string password)
    {
        // Find user by username or student_id
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == usernameOrStudentId || u.StudentId == usernameOrStudentId);

        if (user == null)
            return null;

        // Verify password
        if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
            return null;

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        // Store refresh token in Redis only
        await _redisService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Username = user.Username,
            StudentId = user.StudentId,
            Role = user.Role
        };
    }

    public async Task<RegisterResponse?> RegisterAsync(string studentId, string username, string password, string role = "Student")
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.StudentId == studentId);

        if (existingUser != null)
            return null;

        // Validate role
        if (!new[] { "Student", "Teacher", "Admin" }.Contains(role))
            role = "Student";

        // Hash password
        var passwordHash = PasswordHelper.HashPassword(password);

        // Create new user
        var user = new User
        {
            StudentId = studentId,
            Username = username,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new RegisterResponse
        {
            UserId = user.Id,
            Username = user.Username,
            StudentId = user.StudentId,
            Role = user.Role,
            Message = "User registered successfully"
        };
    }

    public async Task<AuthResponse?> RefreshAccessTokenAsync(string refreshToken)
    {
        // Get user ID from Redis using the refresh token
        var userId = await _redisService.GetUserIdFromRefreshTokenAsync(refreshToken);
        if (userId == null)
            return null;

        // Validate token in Redis
        var isValidInRedis = await _redisService.ValidateRefreshTokenAsync(userId.Value, refreshToken);
        if (!isValidInRedis)
            return null;

        // Get user from database
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return null;

        // Generate new access token
        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Username = user.Username,
            StudentId = user.StudentId,
            Role = user.Role
        };
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        // Revoke refresh token in Redis only
        await _redisService.RevokeRefreshTokenAsync(userId);
        return true;
    }
}

// DTOs
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
