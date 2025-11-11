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

    public async Task<RegisterResponse?> RegisterStudentAsync(StudentRegisterRequest request, bool isAdminCreated = false)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return null;

        var existingEmail = await _studentRepository.EmailExistsAsync(request.Email);
        if (existingEmail)
            return null;

        var courseExists = await _courseRepository.ExistsAsync(request.CourseId);
        if (!courseExists)
            throw new InvalidOperationException($"Course with ID {request.CourseId} does not exist");

        var passwordHash = PasswordHelper.HashPassword(request.Password);
        var enrollmentDate = DateTime.UtcNow;

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = "Student",
            Status = isAdminCreated ? "Active" : "Inactive", // Admin-created students are active, self-registered are inactive
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Create student record first to get database ID
        // Use temporary unique ID to avoid unique constraint violation
        var tempStudentId = $"TEMP-{Guid.NewGuid()}";
        var student = new Student
        {
            UserId = user.Id,
            StudentId = tempStudentId, // Temporary unique value, will be updated
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            YearLevel = request.YearLevel,
            CourseId = request.CourseId,
            EnrollmentDate = enrollmentDate
        };

        await _studentRepository.CreateAsync(student);

        // Generate student ID using enrollment date and database ID
        string generatedStudentId;
        int attempts = 0;
        const int maxAttempts = 10;
        
        do
        {
            generatedStudentId = GenerateStudentId(enrollmentDate, student.Id);
            var exists = await _studentRepository.StudentIdExistsAsync(generatedStudentId);
            if (!exists)
                break;
            attempts++;
            // If collision occurs, wait a millisecond and try again with new random/UUID
            await Task.Delay(1);
        } while (attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            throw new InvalidOperationException("Failed to generate unique student ID after multiple attempts");
        }

        // Update student with generated ID
        student.StudentId = generatedStudentId;
        await _studentRepository.UpdateAsync(student);

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
                var enrollmentDate = DateTime.UtcNow;
                // Use temporary unique ID to avoid unique constraint violation
                var tempStudentId = $"TEMP-{Guid.NewGuid()}";
                var student = new Student
                {
                    UserId = userId,
                    StudentId = tempStudentId, // Temporary unique value, will be updated
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    YearLevel = request.YearLevel,
                    CourseId = request.CourseId,
                    EnrollmentDate = enrollmentDate
                };
                await _studentRepository.CreateAsync(student);
                
                // Generate student ID using enrollment date and database ID
                string generatedStudentId;
                int attempts = 0;
                const int maxAttempts = 10;
                
                do
                {
                    generatedStudentId = GenerateStudentId(enrollmentDate, student.Id);
                    var exists = await _studentRepository.StudentIdExistsAsync(generatedStudentId);
                    if (!exists)
                        break;
                    attempts++;
                    await Task.Delay(1);
                } while (attempts < maxAttempts);

                if (attempts >= maxAttempts)
                {
                    throw new InvalidOperationException("Failed to generate unique student ID after multiple attempts");
                }

                // Update student with generated ID
                student.StudentId = generatedStudentId;
                await _studentRepository.UpdateAsync(student);
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

    private string GenerateStudentId(DateTime enrollmentDate, int studentDatabaseId)
    {
        // Format: ucmn-{YYMMDD}{studentId}{random}
        // Example: ucmn-2511112001 (~15 chars total, 10 digits after ucmn-)
        // - YYMMDD = 6 digits (year/month/day)
        // - studentId = 4 digits (padded, supports up to 9999 students per day)
        // - random = removed to keep it shorter
        
        // 1. Short timestamp: YYMMDD (6 digits)
        var shortDate = enrollmentDate.ToString("yyMMdd");
        
        // 2. Student database ID padded to 4 digits
        var studentIdPart = studentDatabaseId.ToString("D4");
        
        return $"ucmn-{shortDate}{studentIdPart}";
    }
}
