using StudentPeformanceTracker.Helpers;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly JwtService _jwtService;
    private readonly RedisService _redisService;

    public AuthService(
        IUserRepository userRepository,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        IAdminRepository adminRepository,
        ICourseRepository courseRepository,
        JwtService jwtService,
        RedisService redisService)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _adminRepository = adminRepository;
        _courseRepository = courseRepository;
        _jwtService = jwtService;
        _redisService = redisService;
    }

    public async Task<AuthResponse?> LoginAsync(string usernameOrStudentId, string password)
    {
        var user = await _userRepository.GetByUsernameOrStudentIdAsync(usernameOrStudentId);

        if (user == null)
            return null;

        if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
            return null;

        // Check if user is active
        if (user.Status != "Active")
            return null;

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        await _redisService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Username = user.Username,
            StudentId = user.Student?.StudentId ?? "",
            Role = user.Role
        };
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return null;

        var existingEmail = await CheckEmailExistsAsync(request.Email, request.Role);
        if (existingEmail)
            return null;

        if (!new[] { "Student", "Teacher", "Admin" }.Contains(request.Role))
            request.Role = "Student";

        var passwordHash = PasswordHelper.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = request.Role,
            Status = request.Role == "Admin" ? "Active" : "Inactive", // Admins active, others inactive
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        await CreateRoleSpecificRecordAsync(user.Id, request);

        return new RegisterResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = user.Role,
            Message = $"{request.Role} registered successfully"
        };
    }

    public async Task<RegisterResponse?> RegisterStudentAsync(StudentRegisterRequest request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return null;

        var existingEmail = await _studentRepository.EmailExistsAsync(request.Email);
        if (existingEmail)
            return null;

        // Check if student number already exists
        var generatedStudentId = GenerateStudentId(request.StudentNumber);
        var existingStudentId = await _studentRepository.StudentIdExistsAsync(generatedStudentId);
        if (existingStudentId)
            throw new InvalidOperationException($"Student number {generatedStudentId} already exists");

        var courseExists = await _courseRepository.ExistsAsync(request.CourseId);
        if (!courseExists)
            throw new InvalidOperationException($"Course with ID {request.CourseId} does not exist");

        var passwordHash = PasswordHelper.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = "Student",
            Status = "Inactive", // Students are inactive by default
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        var student = new Student
        {
            UserId = user.Id,
            StudentId = generatedStudentId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            YearLevel = request.YearLevel,
            CourseId = request.CourseId,
            EnrollmentDate = DateTime.UtcNow
        };

        await _studentRepository.CreateAsync(student);

        return new RegisterResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "Student",
            Message = "Student registered successfully"
        };
    }

    public async Task<RegisterResponse?> RegisterTeacherAsync(TeacherRegisterRequest request, bool isAdminCreated = false)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return null;

        var existingEmail = await _teacherRepository.EmailExistsAsync(request.Email);
        if (existingEmail)
            return null;

        var passwordHash = PasswordHelper.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = "Teacher",
            Status = isAdminCreated ? "Active" : "Inactive", // Admin-created teachers are active immediately
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        var teacher = new Teacher
        {
            UserId = user.Id,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            HighestQualification = request.HighestQualification,
            Status = request.Status,
            EmergencyContact = request.EmergencyContact,
            EmergencyPhone = request.EmergencyPhone,
            HireDate = request.HireDate?.ToUniversalTime() ?? DateTime.UtcNow
            // Note: Departments will be assigned by admins later via TeacherDepartments junction table
        };

        await _teacherRepository.CreateAsync(teacher);

        return new RegisterResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "Teacher",
            Message = isAdminCreated ? "Teacher created successfully with Active status" : "Teacher registered successfully"
        };
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        // Check email across all roles
        var teacherExists = await _teacherRepository.EmailExistsAsync(email);
        if (teacherExists) return true;

        var studentExists = await _studentRepository.EmailExistsAsync(email);
        if (studentExists) return true;

        var adminExists = await _adminRepository.EmailExistsAsync(email);
        return adminExists;
    }

    public async Task<RegisterResponse?> RegisterAdminAsync(AdminRegisterRequest request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return null;

        var existingEmail = await _adminRepository.EmailExistsAsync(request.Email);
        if (existingEmail)
            return null;

        var passwordHash = PasswordHelper.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = "Admin",
            Status = "Active", // Admins are active immediately
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        var admin = new Admin
        {
            UserId = user.Id,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone
        };

        await _adminRepository.CreateAsync(admin);

        return new RegisterResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "Admin",
            Message = "Admin registered successfully"
        };
    }

    public async Task<AuthResponse?> RefreshAccessTokenAsync(string refreshToken)
    {
        var userId = await _redisService.GetUserIdFromRefreshTokenAsync(refreshToken);
        if (userId == null)
            return null;

        var isValidInRedis = await _redisService.ValidateRefreshTokenAsync(userId.Value, refreshToken);
        if (!isValidInRedis)
            return null;

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
            return null;

        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Username = user.Username,
            StudentId = user.Student?.StudentId ?? "",
            Role = user.Role
        };
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        await _redisService.RevokeRefreshTokenAsync(userId);
        return true;
    }

    private async Task<bool> CheckEmailExistsAsync(string email, string role)
    {
        return role switch
        {
            "Student" => await _studentRepository.EmailExistsAsync(email),
            "Teacher" => await _teacherRepository.EmailExistsAsync(email),
            "Admin" => await _adminRepository.EmailExistsAsync(email),
            _ => false
        };
    }

    private async Task CreateRoleSpecificRecordAsync(int userId, RegisterRequest request)
    {
        switch (request.Role)
        {
            case "Student":
                var student = new Student
                {
                    UserId = userId,
                    StudentId = GenerateStudentId(request.StudentNumber!),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    YearLevel = request.YearLevel,
                    CourseId = request.CourseId,
                    EnrollmentDate = DateTime.UtcNow
                };
                await _studentRepository.CreateAsync(student);
                break;

            case "Teacher":
                var teacher = new Teacher
                {
                    UserId = userId,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    HireDate = DateTime.UtcNow
                };
                await _teacherRepository.CreateAsync(teacher);
                break;

            case "Admin":
                var admin = new Admin
                {
                    UserId = userId,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone
                };
                await _adminRepository.CreateAsync(admin);
                break;
        }
    }

    private string GenerateStudentId(string studentNumber)
    {
        return $"ucmn-{studentNumber}";
    }
}
