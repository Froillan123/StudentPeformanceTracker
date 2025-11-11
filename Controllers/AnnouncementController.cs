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
[Route("api/v{version:apiVersion}/announcement")]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ISectionSubjectRepository _sectionSubjectRepository;
    private readonly IStudentSubjectRepository _studentSubjectRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAdminRepository _adminRepository;

    public AnnouncementController(
        IAnnouncementRepository announcementRepository,
        ITeacherRepository teacherRepository,
        ISectionSubjectRepository sectionSubjectRepository,
        IStudentSubjectRepository studentSubjectRepository,
        IStudentRepository studentRepository,
        IAdminRepository adminRepository)
    {
        _announcementRepository = announcementRepository;
        _teacherRepository = teacherRepository;
        _sectionSubjectRepository = sectionSubjectRepository;
        _studentSubjectRepository = studentSubjectRepository;
        _studentRepository = studentRepository;
        _adminRepository = adminRepository;
    }

    /// <summary>
    /// Get announcements for current user (filtered by their classes)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Student,Teacher")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllActive()
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Get user role
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var role = roleClaim?.Value ?? "";

            IEnumerable<Announcement> announcements;

            if (role == "Student")
            {
                // Get student by userId
                var student = await _studentRepository.GetByUserIdAsync(userId);
                if (student == null)
                {
                    return Unauthorized(new { message = "Student profile not found" });
                }

                // Get student's enrolled classes
                var studentSubjects = await _studentSubjectRepository.GetByStudentIdAsync(student.Id);
                var sectionSubjectIds = studentSubjects
                    .Where(ss => ss.Status == "Enrolled" || ss.Status == "Pending")
                    .Select(ss => ss.SectionSubjectId)
                    .ToList();

                // Get section-specific announcements
                var sectionAnnouncements = sectionSubjectIds.Any() 
                    ? await _announcementRepository.GetBySectionSubjectIdsAsync(sectionSubjectIds)
                    : new List<Announcement>();

                // Get general announcements (admin-created)
                var generalAnnouncements = await _announcementRepository.GetAllGeneralAnnouncementsAsync();

                // Combine both types
                announcements = sectionAnnouncements.Concat(generalAnnouncements);
            }
            else if (role == "Teacher")
            {
                // Get teacher's classes
                var teacher = await _teacherRepository.GetByUserIdAsync(userId);
                if (teacher == null)
                {
                    return Unauthorized(new { message = "Teacher profile not found" });
                }

                var teacherClasses = await _sectionSubjectRepository.GetByTeacherIdAsync(teacher.Id);
                var sectionSubjectIds = teacherClasses.Select(ss => ss.Id).ToList();

                // Get section-specific announcements
                var sectionAnnouncements = sectionSubjectIds.Any() 
                    ? await _announcementRepository.GetBySectionSubjectIdsAsync(sectionSubjectIds)
                    : new List<Announcement>();

                // Get general announcements (admin-created)
                var generalAnnouncements = await _announcementRepository.GetAllGeneralAnnouncementsAsync();

                // Combine both types
                announcements = sectionAnnouncements.Concat(generalAnnouncements);
            }
            else
            {
                return Unauthorized(new { message = "Invalid role" });
            }

            var result = announcements.Select(a => new
            {
                a.Id,
                a.TeacherId,
                TeacherName = a.Teacher != null ? $"{a.Teacher.FirstName} {a.Teacher.LastName}" : null,
                a.AdminId,
                AdminName = a.Admin != null ? $"{a.Admin.FirstName} {a.Admin.LastName}" : null,
                a.SectionSubjectId,
                SectionName = a.SectionSubject?.Section?.SectionName ?? (a.AdminId != null ? "General" : "Unknown"),
                SubjectName = a.SectionSubject?.Subject?.SubjectName ?? (a.AdminId != null ? "All Students" : "Unknown"),
                a.Title,
                a.Content,
                a.Priority,
                a.IsActive,
                IsGeneral = a.AdminId != null,
                a.CreatedAt,
                a.UpdatedAt
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving announcements", error = ex.Message });
        }
    }

    /// <summary>
    /// Get announcement by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        try
        {
            var announcement = await _announcementRepository.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound(new { message = "Announcement not found" });
            }

            var result = new
            {
                announcement.Id,
                announcement.TeacherId,
                TeacherName = announcement.Teacher != null ? $"{announcement.Teacher.FirstName} {announcement.Teacher.LastName}" : null,
                announcement.AdminId,
                AdminName = announcement.Admin != null ? $"{announcement.Admin.FirstName} {announcement.Admin.LastName}" : null,
                announcement.SectionSubjectId,
                SectionName = announcement.SectionSubject?.Section?.SectionName ?? (announcement.AdminId != null ? "General" : "Unknown"),
                SubjectName = announcement.SectionSubject?.Subject?.SubjectName ?? (announcement.AdminId != null ? "All Students" : "Unknown"),
                announcement.Title,
                announcement.Content,
                announcement.Priority,
                announcement.IsActive,
                IsGeneral = announcement.AdminId != null,
                announcement.CreatedAt,
                announcement.UpdatedAt
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Get announcements by teacher ID
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<IEnumerable<object>>> GetByTeacherId(int teacherId)
    {
        try
        {
            var announcements = await _announcementRepository.GetByTeacherIdAsync(teacherId);
            var result = announcements.Select(a => new
            {
                a.Id,
                a.TeacherId,
                TeacherName = a.Teacher != null ? $"{a.Teacher.FirstName} {a.Teacher.LastName}" : "Unknown",
                a.SectionSubjectId,
                SectionName = a.SectionSubject?.Section?.SectionName ?? "Unknown",
                SubjectName = a.SectionSubject?.Subject?.SubjectName ?? "Unknown",
                a.Title,
                a.Content,
                a.Priority,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving announcements", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new announcement (Teacher only)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<object>> Create([FromBody] CreateAnnouncementRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current teacher from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return Unauthorized(new { message = "Teacher profile not found" });
            }

            // Validate priority
            if (!new[] { "General", "Important", "Urgent" }.Contains(request.Priority))
            {
                return BadRequest(new { message = "Priority must be General, Important, or Urgent" });
            }

            // Validate that the teacher teaches this section subject
            var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(request.SectionSubjectId);
            if (sectionSubject == null)
            {
                return BadRequest(new { message = "Section subject not found" });
            }

            if (sectionSubject.TeacherId != teacher.Id)
            {
                return Forbid("You can only create announcements for your own classes");
            }

            var announcement = new Announcement
            {
                TeacherId = teacher.Id,
                SectionSubjectId = request.SectionSubjectId,
                Title = request.Title,
                Content = request.Content,
                Priority = request.Priority,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _announcementRepository.CreateAsync(announcement);
            
            // Reload with teacher data
            var reloaded = await _announcementRepository.GetByIdAsync(created.Id);

            var result = new
            {
                reloaded!.Id,
                reloaded.TeacherId,
                TeacherName = reloaded.Teacher != null ? $"{reloaded.Teacher.FirstName} {reloaded.Teacher.LastName}" : "Unknown",
                reloaded.SectionSubjectId,
                SectionName = reloaded.SectionSubject?.Section?.SectionName ?? "Unknown",
                SubjectName = reloaded.SectionSubject?.Subject?.SubjectName ?? "Unknown",
                reloaded.Title,
                reloaded.Content,
                reloaded.Priority,
                reloaded.IsActive,
                reloaded.CreatedAt,
                reloaded.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an announcement (Teacher only, only own announcements)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<object>> Update(int id, [FromBody] UpdateAnnouncementRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current teacher from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return Unauthorized(new { message = "Teacher profile not found" });
            }

            var announcement = await _announcementRepository.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound(new { message = "Announcement not found" });
            }

            // Check if teacher owns this announcement
            if (announcement.TeacherId != teacher.Id)
            {
                return Forbid("You can only update your own announcements");
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.Title))
            {
                announcement.Title = request.Title;
            }
            if (!string.IsNullOrEmpty(request.Content))
            {
                announcement.Content = request.Content;
            }
            if (!string.IsNullOrEmpty(request.Priority))
            {
                if (!new[] { "General", "Important", "Urgent" }.Contains(request.Priority))
                {
                    return BadRequest(new { message = "Priority must be General, Important, or Urgent" });
                }
                announcement.Priority = request.Priority;
            }
            if (request.IsActive.HasValue)
            {
                announcement.IsActive = request.IsActive.Value;
            }

            // Ensure DateTime values are UTC for PostgreSQL
            announcement.UpdatedAt = DateTime.UtcNow;
            if (announcement.CreatedAt.Kind != DateTimeKind.Utc)
            {
                announcement.CreatedAt = announcement.CreatedAt.ToUniversalTime();
            }

            var updated = await _announcementRepository.UpdateAsync(announcement);
            
            // Reload with teacher data
            var reloaded = await _announcementRepository.GetByIdAsync(updated.Id);

            var result = new
            {
                reloaded!.Id,
                reloaded.TeacherId,
                TeacherName = reloaded.Teacher != null ? $"{reloaded.Teacher.FirstName} {reloaded.Teacher.LastName}" : "Unknown",
                reloaded.SectionSubjectId,
                SectionName = reloaded.SectionSubject?.Section?.SectionName ?? "Unknown",
                SubjectName = reloaded.SectionSubject?.Subject?.SubjectName ?? "Unknown",
                reloaded.Title,
                reloaded.Content,
                reloaded.Priority,
                reloaded.IsActive,
                reloaded.CreatedAt,
                reloaded.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an announcement (Teacher only, only own announcements)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            // Get current teacher from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return Unauthorized(new { message = "Teacher profile not found" });
            }

            var announcement = await _announcementRepository.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound(new { message = "Announcement not found" });
            }

            // Check if teacher owns this announcement
            if (announcement.TeacherId != teacher.Id)
            {
                return Forbid("You can only delete your own announcements");
            }

            var success = await _announcementRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Announcement not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all general announcements (Admin only)
    /// </summary>
    [HttpGet("admin/general")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllGeneralAnnouncements()
    {
        try
        {
            var announcements = await _announcementRepository.GetAllGeneralAnnouncementsAsync();
            var result = announcements.Select(a => new
            {
                a.Id,
                a.AdminId,
                AdminName = a.Admin != null ? $"{a.Admin.FirstName} {a.Admin.LastName}" : "Unknown",
                a.Title,
                a.Content,
                a.Priority,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving general announcements", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a general announcement (Admin only)
    /// </summary>
    [HttpPost("admin/general")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> CreateGeneralAnnouncement([FromBody] CreateGeneralAnnouncementRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current admin from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var admin = await _adminRepository.GetByUserIdAsync(userId);
            if (admin == null)
            {
                return Unauthorized(new { message = "Admin profile not found" });
            }

            // Validate priority
            if (!new[] { "General", "Important", "Urgent" }.Contains(request.Priority))
            {
                return BadRequest(new { message = "Priority must be General, Important, or Urgent" });
            }

            var announcement = new Announcement
            {
                AdminId = admin.Id,
                TeacherId = null,
                SectionSubjectId = null,
                Title = request.Title,
                Content = request.Content,
                Priority = request.Priority,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _announcementRepository.CreateAsync(announcement);
            
            // Reload with admin data
            var reloaded = await _announcementRepository.GetByIdAsync(created.Id);

            var result = new
            {
                reloaded!.Id,
                reloaded.AdminId,
                AdminName = reloaded.Admin != null ? $"{reloaded.Admin.FirstName} {reloaded.Admin.LastName}" : "Unknown",
                reloaded.Title,
                reloaded.Content,
                reloaded.Priority,
                reloaded.IsActive,
                reloaded.CreatedAt,
                reloaded.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating general announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a general announcement (Admin only, only own announcements)
    /// </summary>
    [HttpPut("admin/general/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> UpdateGeneralAnnouncement(int id, [FromBody] UpdateGeneralAnnouncementRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current admin from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var admin = await _adminRepository.GetByUserIdAsync(userId);
            if (admin == null)
            {
                return Unauthorized(new { message = "Admin profile not found" });
            }

            var announcement = await _announcementRepository.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound(new { message = "Announcement not found" });
            }

            // Check if this is a general announcement
            if (announcement.AdminId == null)
            {
                return BadRequest(new { message = "This is not a general announcement" });
            }

            // Check if admin owns this announcement
            if (announcement.AdminId != admin.Id)
            {
                return Forbid("You can only update your own general announcements");
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.Title))
            {
                announcement.Title = request.Title;
            }
            if (!string.IsNullOrEmpty(request.Content))
            {
                announcement.Content = request.Content;
            }
            if (!string.IsNullOrEmpty(request.Priority))
            {
                if (!new[] { "General", "Important", "Urgent" }.Contains(request.Priority))
                {
                    return BadRequest(new { message = "Priority must be General, Important, or Urgent" });
                }
                announcement.Priority = request.Priority;
            }
            if (request.IsActive.HasValue)
            {
                announcement.IsActive = request.IsActive.Value;
            }

            // Ensure DateTime values are UTC for PostgreSQL
            announcement.UpdatedAt = DateTime.UtcNow;
            if (announcement.CreatedAt.Kind != DateTimeKind.Utc)
            {
                announcement.CreatedAt = announcement.CreatedAt.ToUniversalTime();
            }

            var updated = await _announcementRepository.UpdateAsync(announcement);
            
            // Reload with admin data
            var reloaded = await _announcementRepository.GetByIdAsync(updated.Id);

            var result = new
            {
                reloaded!.Id,
                reloaded.AdminId,
                AdminName = reloaded.Admin != null ? $"{reloaded.Admin.FirstName} {reloaded.Admin.LastName}" : "Unknown",
                reloaded.Title,
                reloaded.Content,
                reloaded.Priority,
                reloaded.IsActive,
                reloaded.CreatedAt,
                reloaded.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating general announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a general announcement (Admin only, only own announcements)
    /// </summary>
    [HttpDelete("admin/general/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteGeneralAnnouncement(int id)
    {
        try
        {
            // Get current admin from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var admin = await _adminRepository.GetByUserIdAsync(userId);
            if (admin == null)
            {
                return Unauthorized(new { message = "Admin profile not found" });
            }

            var announcement = await _announcementRepository.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound(new { message = "Announcement not found" });
            }

            // Check if this is a general announcement
            if (announcement.AdminId == null)
            {
                return BadRequest(new { message = "This is not a general announcement" });
            }

            // Check if admin owns this announcement
            if (announcement.AdminId != admin.Id)
            {
                return Forbid("You can only delete your own general announcements");
            }

            var success = await _announcementRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Announcement not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting general announcement", error = ex.Message });
        }
    }
}

public class CreateAnnouncementRequest
{
    [Required]
    public int SectionSubjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "General";
}

public class UpdateAnnouncementRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Content { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    public bool? IsActive { get; set; }
}

public class CreateGeneralAnnouncementRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "General";
}

public class UpdateGeneralAnnouncementRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Content { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    public bool? IsActive { get; set; }
}

