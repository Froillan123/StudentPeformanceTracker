using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Services;
using StudentPeformanceTracker.Repository;
using StudentPeformanceTracker.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using System.Text;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 5199
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5199, listenOptions =>
    {
        // listenOptions.UseHttps(); // Commented out for local development
    });
});

// Database configuration
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Convert PostgreSQL URL format to connection string format if needed
string connectionString;
if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgresql://"))
{
    // Parse PostgreSQL URL format: postgresql://user:pass@host:port/db?params
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo.Length > 0 ? userInfo[0] : "";
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    
    // Handle port - use 5432 as default if not specified
    var port = uri.Port == -1 ? 5432 : uri.Port;
    
    // Check if this is a Neon database (requires SSL) or local database
    var isNeonDatabase = uri.Host.Contains("neon.tech") || uri.Host.Contains("aws.neon.tech");
    var sslMode = isNeonDatabase ? "Require" : (builder.Environment.IsDevelopment() ? "Disable" : "Require");
    connectionString = $"Host={uri.Host};Database={uri.AbsolutePath.TrimStart('/')};Username={username};Password={password};Port={port};SslMode={sslMode}";
}
else
{
    connectionString = databaseUrl ?? "";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(120);
        npgsqlOptions.EnableRetryOnFailure(3);
    }));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

// Register new repositories for course and subject management
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IYearLevelRepository, YearLevelRepository>();
builder.Services.AddScoped<ICourseSubjectRepository, CourseSubjectRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ISectionSubjectRepository, SectionSubjectRepository>();
builder.Services.AddScoped<IStudentSubjectRepository, StudentSubjectRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ITeacherSubjectRepository, TeacherSubjectRepository>();

// Register services
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserManagementService>();

// Register new services for course and subject management
builder.Services.AddScoped<CourseSubjectService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<SectionService>();
builder.Services.AddScoped<EnrollmentService>();

// JWT Authentication configuration
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET not found");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "StudentPerformanceTracker";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "StudentPerformanceTrackerUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT to read from HttpOnly cookies instead of Authorization header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Try to get token from cookie first
            var accessToken = context.Request.Cookies["access_token"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Teacher", "Admin"));
});

// Add Controllers for API endpoints with JSON options to handle circular references
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
}).AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Register API Configuration singleton
builder.Services.AddSingleton(StudentPeformanceTracker.Configuration.ApiConfiguration.LoadFromEnvironment());

// Add HttpClient for API calls from Razor Pages
builder.Services.AddHttpClient("default", client =>
{
    // Configure for development - ignore SSL certificate errors
    client.DefaultRequestHeaders.Add("User-Agent", "StudentPerformanceTracker/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // In development, ignore SSL certificate errors
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }

    return handler;
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Performance Tracker API",
        Version = "v1",
        Description = "API for Student Performance Tracker with JWT Authentication"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for easier API testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Performance Tracker API v1");
    c.RoutePrefix = "swagger"; // Access at https://localhost:5199/swagger
});

if (app.Environment.IsDevelopment())
{
    // Development-specific configurations can go here
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// TODO: Uncomment when you want to initialize database
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     DbInitializer.Initialize(db);
// }

// app.UseHttpsRedirection(); // Commented out for local development

app.UseRouting();

app.UseAuthentication(); // Must come before UseAuthorization

// Custom middleware to handle unauthorized requests and redirect to login
app.Use(async (context, next) =>
{
    await next();
    
    // If we get a 401 Unauthorized response, redirect to login page
    if (context.Response.StatusCode == 401)
    {
        // Only redirect for page requests, not API requests
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            // For API requests, keep the 401 response
            return;
        }
        
        // For page requests, redirect to login with a message
        context.Response.Redirect("/LoginPage?message=Please login to access this page");
    }
});

app.UseAuthorization();

app.MapControllers(); // Map API controllers

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Redirect old routes to new organized structure
app.MapGet("/StudentDashboard", context =>
{
    context.Response.Redirect("/Student/Dashboard");
    return Task.CompletedTask;
});

app.MapGet("/TeacherDashboard", context =>
{
    context.Response.Redirect("/Teacher/Dashboard");
    return Task.CompletedTask;
});

app.MapGet("/AdminDashboard", context =>
{
    context.Response.Redirect("/Admin/Dashboard");
    return Task.CompletedTask;
});

app.MapGet("/AdminAnalytics", context =>
{
    context.Response.Redirect("/Admin/Analytics");
    return Task.CompletedTask;
});

app.MapGet("/AdminCourseManage", context =>
{
    context.Response.Redirect("/Admin/CourseManage");
    return Task.CompletedTask;
});

app.MapGet("/AdminStudentManage", context =>
{
    context.Response.Redirect("/Admin/StudentManage");
    return Task.CompletedTask;
});

app.MapGet("/AdminTeacherManage", context =>
{
    context.Response.Redirect("/Admin/TeacherManage");
    return Task.CompletedTask;
});

app.MapGet("/AdminUserManage", context =>
{
    context.Response.Redirect("/Admin/UserManage");
    return Task.CompletedTask;
});

app.MapGet("/StudentAnnouncements", context =>
{
    context.Response.Redirect("/Student/Announcements");
    return Task.CompletedTask;
});

app.MapGet("/StudentGrades", context =>
{
    context.Response.Redirect("/Student/Grades");
    return Task.CompletedTask;
});

app.MapGet("/MyClasses", context =>
{
    context.Response.Redirect("/Student/MyClasses");
    return Task.CompletedTask;
});

app.MapGet("/Announcements", context =>
{
    context.Response.Redirect("/Student/Announcements");
    return Task.CompletedTask;
});

// Removed generic /Profile redirect since we now have role-specific profile pages
// /Student/Profile, /Teacher/Profile, /Admin/Profile

// TESTING ROUTE HERE
app.MapGet("/", context =>
{
     context.Response.Redirect("/LoginPage");
     return Task.CompletedTask;
});

app.Run();
