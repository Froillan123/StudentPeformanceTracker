using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Data;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/sectionsubject")]
    public class SectionSubjectController : ControllerBase
    {
        private readonly ISectionSubjectRepository _sectionSubjectRepository;
        private readonly ITeacherSubjectRepository _teacherSubjectRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly AppDbContext _context;

        public SectionSubjectController(
            ISectionSubjectRepository sectionSubjectRepository,
            ITeacherSubjectRepository teacherSubjectRepository,
            ISectionRepository sectionRepository,
            AppDbContext context)
        {
            _sectionSubjectRepository = sectionSubjectRepository;
            _teacherSubjectRepository = teacherSubjectRepository;
            _sectionRepository = sectionRepository;
            _context = context;
        }

        /// <summary>
        /// Get section subject by EDP code (for student enrollment)
        /// </summary>
        [HttpGet("edpcode/{edpCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetByEdpCode(string edpCode)
        {
            try
            {
                var sectionSubject = await _sectionSubjectRepository.GetByEdpCodeAsync(edpCode);
                if (sectionSubject == null)
                {
                    return NotFound(new { message = "Class not found with this EDP code" });
                }

                var result = new
                {
                    sectionSubject.Id,
                    sectionSubject.SectionId,
                    SectionName = sectionSubject.Section?.SectionName,
                    sectionSubject.SubjectId,
                    SubjectName = sectionSubject.Subject?.SubjectName,
                    SubjectDescription = sectionSubject.Subject?.Description,
                    sectionSubject.TeacherId,
                    TeacherName = sectionSubject.Teacher != null
                        ? $"{sectionSubject.Teacher.FirstName} {sectionSubject.Teacher.LastName}"
                        : "TBA",
                    sectionSubject.EdpCode,
                    sectionSubject.ScheduleDay,
                    sectionSubject.ScheduleTime,
                    sectionSubject.Room,
                    sectionSubject.MaxStudents,
                    sectionSubject.CurrentEnrollment,
                    sectionSubject.IsActive
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving section subject", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available section subjects filtered by course, year level, and semester
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableSectionSubjects(
            [FromQuery] int courseId, 
            [FromQuery] int yearLevelId, 
            [FromQuery] int semesterId)
        {
            try
            {
                // 1. Get sections for the course/year/semester
                var sections = await _sectionRepository.GetByCourseYearSemesterAsync(courseId, yearLevelId, semesterId);
                var sectionIds = sections.Select(s => s.Id).ToList();
                
                if (!sectionIds.Any())
                {
                    return Ok(new List<object>()); // No sections found for this course/year/semester
                }
                
                // 2. Get all section subjects for those sections
                var allSectionSubjects = await _sectionSubjectRepository.GetAllAsync();
                var filtered = allSectionSubjects.Where(ss => sectionIds.Contains(ss.SectionId) && ss.IsActive);
                
                // 3. Return with details
                var result = filtered.Select(ss => new
                {
                    ss.Id,
                    ss.SectionId,
                    SectionName = ss.Section?.SectionName,
                    ss.SubjectId,
                    SubjectName = ss.Subject?.SubjectName,
                    SubjectDescription = ss.Subject?.Description,
                    ss.TeacherId,
                    TeacherName = ss.Teacher != null ? $"{ss.Teacher.FirstName} {ss.Teacher.LastName}" : "TBA",
                    ss.ScheduleDay,
                    ss.ScheduleTime,
                    Schedule = !string.IsNullOrEmpty(ss.ScheduleDay) && !string.IsNullOrEmpty(ss.ScheduleTime) 
                        ? $"{ss.ScheduleDay} {ss.ScheduleTime}" 
                        : "TBA",
                    ss.Room,
                    ss.EdpCode,
                    ss.MaxStudents,
                    ss.CurrentEnrollment,
                    AvailableSlots = ss.MaxStudents - ss.CurrentEnrollment,
                    ss.IsActive
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving available section subjects", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all section subjects (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            try
            {
                var sectionSubjects = await _sectionSubjectRepository.GetAllAsync();
                var result = sectionSubjects.Select(ss => new
                {
                    ss.Id,
                    ss.SectionId,
                    SectionName = ss.Section?.SectionName,
                    ss.SubjectId,
                    SubjectName = ss.Subject?.SubjectName,
                    SubjectDescription = ss.Subject?.Description,
                    ss.TeacherId,
                    TeacherName = ss.Teacher != null ? $"{ss.Teacher.FirstName} {ss.Teacher.LastName}" : "N/A",
                    ss.ScheduleDay,
                    ss.ScheduleTime,
                    ss.Room,
                    ss.EdpCode,
                    ss.MaxStudents,
                    ss.CurrentEnrollment,
                    ss.IsActive,
                    ss.CreatedAt,
                    ss.UpdatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving section subjects", error = ex.Message });
            }
        }

        /// <summary>
        /// Get section subject by ID (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            try
            {
                var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(id);
                if (sectionSubject == null)
                {
                    return NotFound(new { message = "Section subject not found" });
                }

                var result = new
                {
                    sectionSubject.Id,
                    sectionSubject.SectionId,
                    SectionName = sectionSubject.Section?.SectionName,
                    sectionSubject.SubjectId,
                    SubjectName = sectionSubject.Subject?.SubjectName,
                    SubjectDescription = sectionSubject.Subject?.Description,
                    sectionSubject.TeacherId,
                    TeacherName = sectionSubject.Teacher != null ? $"{sectionSubject.Teacher.FirstName} {sectionSubject.Teacher.LastName}" : "N/A",
                    sectionSubject.ScheduleDay,
                    sectionSubject.ScheduleTime,
                    sectionSubject.Room,
                    sectionSubject.EdpCode,
                    sectionSubject.MaxStudents,
                    sectionSubject.CurrentEnrollment,
                    sectionSubject.IsActive,
                    sectionSubject.CreatedAt,
                    sectionSubject.UpdatedAt
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving section subject", error = ex.Message });
            }
        }

        /// <summary>
        /// Get section subjects by section ID (Admin only)
        /// </summary>
        [HttpGet("section/{sectionId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<object>>> GetBySectionId(int sectionId)
        {
            try
            {
                var sectionSubjects = await _sectionSubjectRepository.GetBySectionIdAsync(sectionId);
                var result = sectionSubjects.Select(ss => new
                {
                    ss.Id,
                    ss.SectionId,
                    SectionName = ss.Section?.SectionName,
                    ss.SubjectId,
                    SubjectName = ss.Subject?.SubjectName,
                    SubjectDescription = ss.Subject?.Description,
                    ss.TeacherId,
                    TeacherName = ss.Teacher != null ? $"{ss.Teacher.FirstName} {ss.Teacher.LastName}" : "N/A",
                    ss.ScheduleDay,
                    ss.ScheduleTime,
                    ss.Room,
                    ss.EdpCode,
                    ss.MaxStudents,
                    ss.CurrentEnrollment,
                    ss.IsActive,
                    ss.CreatedAt,
                    ss.UpdatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving section subjects", error = ex.Message });
            }
        }

        /// <summary>
        /// Get section subjects by teacher ID
        /// </summary>
        [HttpGet("teacher/{teacherId}")]
        [Authorize(Policy = "TeacherOnly")]
        public async Task<ActionResult<IEnumerable<object>>> GetByTeacherId(int teacherId)
        {
            try
            {
                var sectionSubjects = await _sectionSubjectRepository.GetByTeacherIdAsync(teacherId);
                
                // Get actual student counts for each section subject
                var sectionSubjectIds = sectionSubjects.Select(ss => ss.Id).ToList();
                var studentCounts = await _context.StudentSubjects
                    .Where(ss => sectionSubjectIds.Contains(ss.SectionSubjectId) && 
                           (ss.Status == "Enrolled" || ss.Status == "Pending"))
                    .GroupBy(ss => ss.SectionSubjectId)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
                
                var result = sectionSubjects.Select(ss => new
                {
                    ss.Id,
                    ss.SectionId,
                    SectionName = ss.Section?.SectionName,
                    ss.SubjectId,
                    SubjectName = ss.Subject?.SubjectName,
                    SubjectDescription = ss.Subject?.Description,
                    ss.TeacherId,
                    TeacherName = ss.Teacher != null ? $"{ss.Teacher.FirstName} {ss.Teacher.LastName}" : "N/A",
                    Schedule = !string.IsNullOrEmpty(ss.ScheduleDay) && !string.IsNullOrEmpty(ss.ScheduleTime) 
                        ? $"{ss.ScheduleDay} {ss.ScheduleTime}" 
                        : "TBA",
                    ss.ScheduleDay,
                    ss.ScheduleTime,
                    ss.Room,
                    ss.EdpCode,
                    ss.MaxStudents,
                    ss.CurrentEnrollment,
                    StudentCount = studentCounts.GetValueOrDefault(ss.Id, 0),
                    ss.IsActive,
                    ss.CreatedAt,
                    ss.UpdatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving section subjects", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new section subject (assign subject to section) - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<object>> Create([FromBody] CreateSectionSubjectRequest request)
        {
            try
            {
                // Check if this subject is already assigned to this section
                var existingSectionSubjects = await _sectionSubjectRepository.GetBySectionIdAsync(request.SectionId);
                if (existingSectionSubjects.Any(ss => ss.SubjectId == request.SubjectId))
                {
                    return BadRequest(new { message = "This subject is already assigned to this section." });
                }

                // Generate unique EDP code
                string edpCode = await GenerateUniqueEdpCodeAsync();

                var sectionSubject = new SectionSubject
                {
                    SectionId = request.SectionId,
                    SubjectId = request.SubjectId,
                    TeacherId = request.TeacherId,
                    ScheduleDay = ExtractScheduleDay(request.Schedule),
                    ScheduleTime = ExtractScheduleTime(request.Schedule),
                    Room = request.Room,
                    EdpCode = edpCode,
                    MaxStudents = request.MaxStudents ?? 40,
                    CurrentEnrollment = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _sectionSubjectRepository.CreateAsync(sectionSubject);

                // Automatically create TeacherSubject record to link teacher to this section subject
                if (request.TeacherId > 0)
                {
                    var teacherSubject = new TeacherSubject
                    {
                        TeacherId = request.TeacherId,
                        SectionSubjectId = created.Id,
                        IsPrimary = true, // Mark as primary teacher
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _teacherSubjectRepository.CreateAsync(teacherSubject);
                }

                var result = new
                {
                    created.Id,
                    created.SectionId,
                    created.SubjectId,
                    created.TeacherId,
                    Schedule = !string.IsNullOrEmpty(created.ScheduleDay) && !string.IsNullOrEmpty(created.ScheduleTime) 
                        ? $"{created.ScheduleDay} {created.ScheduleTime}" 
                        : request.Schedule,
                    created.ScheduleDay,
                    created.ScheduleTime,
                    created.Room,
                    created.EdpCode,
                    created.MaxStudents,
                    created.CurrentEnrollment,
                    created.IsActive,
                    created.CreatedAt,
                    created.UpdatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating section subject", error = ex.Message });
            }
        }

        /// <summary>
        /// Update a section subject (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<object>> Update(int id, [FromBody] UpdateSectionSubjectRequest request)
        {
            try
            {
                var existing = await _sectionSubjectRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Section subject not found" });
                }

                // Track if teacher changed
                int? oldTeacherId = existing.TeacherId;
                int? newTeacherId = request.TeacherId;

                existing.TeacherId = request.TeacherId ?? existing.TeacherId;
                if (!string.IsNullOrEmpty(request.Schedule))
                {
                    existing.ScheduleDay = ExtractScheduleDay(request.Schedule);
                    existing.ScheduleTime = ExtractScheduleTime(request.Schedule);
                }
                existing.Room = request.Room ?? existing.Room;
                existing.MaxStudents = request.MaxStudents ?? existing.MaxStudents;
                existing.IsActive = request.IsActive ?? existing.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                var updated = await _sectionSubjectRepository.UpdateAsync(existing);

                // Update TeacherSubject if teacher changed
                if (newTeacherId.HasValue && oldTeacherId != newTeacherId)
                {
                    // Remove old teacher assignment if exists
                    if (oldTeacherId.HasValue)
                    {
                        await _teacherSubjectRepository.DeleteByTeacherAndSectionSubjectAsync(oldTeacherId.Value, id);
                    }

                    // Add new teacher assignment
                    var teacherSubject = new TeacherSubject
                    {
                        TeacherId = newTeacherId.Value,
                        SectionSubjectId = id,
                        IsPrimary = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _teacherSubjectRepository.CreateAsync(teacherSubject);
                }

                var result = new
                {
                    updated.Id,
                    updated.SectionId,
                    updated.SubjectId,
                    updated.TeacherId,
                    Schedule = !string.IsNullOrEmpty(updated.ScheduleDay) && !string.IsNullOrEmpty(updated.ScheduleTime) 
                        ? $"{updated.ScheduleDay} {updated.ScheduleTime}" 
                        : "TBA",
                    updated.ScheduleDay,
                    updated.ScheduleTime,
                    updated.Room,
                    updated.EdpCode,
                    updated.MaxStudents,
                    updated.CurrentEnrollment,
                    updated.IsActive,
                    updated.CreatedAt,
                    updated.UpdatedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating section subject", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a section subject (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // First check if the section subject exists
                var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(id);
                if (sectionSubject == null)
                {
                    return NotFound(new { message = "Section subject not found" });
                }

                // Delete all TeacherSubject records for this section subject
                var teacherSubjects = await _teacherSubjectRepository.GetBySectionSubjectIdAsync(id);
                foreach (var ts in teacherSubjects)
                {
                    await _teacherSubjectRepository.DeleteAsync(ts.Id);
                }

                // Now delete the section subject
                var success = await _sectionSubjectRepository.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Section subject not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting section subject", error = ex.Message });
            }
        }

        private async Task<string> GenerateUniqueEdpCodeAsync()
        {
            string edpCode;
            bool exists;
            
            do
            {
                edpCode = GenerateRandomEdpCode();
                var existing = await _sectionSubjectRepository.GetByEdpCodeAsync(edpCode);
                exists = existing != null;
            } while (exists);

            return edpCode;
        }

        private static string GenerateRandomEdpCode()
        {
            var random = new Random();
            return random.Next(10000, 99999).ToString();
        }

        private static string? ExtractScheduleDay(string? schedule)
        {
            if (string.IsNullOrWhiteSpace(schedule)) return null;
            
            // Try to extract day from schedule format like "Mon/Wed 10:00-11:00"
            var parts = schedule.Split(new[] { ' ' }, 2);
            return parts.Length > 0 ? parts[0] : null;
        }

        private static string? ExtractScheduleTime(string? schedule)
        {
            if (string.IsNullOrWhiteSpace(schedule)) return null;
            
            // Try to extract time from schedule format like "Mon/Wed 10:00-11:00"
            var parts = schedule.Split(new[] { ' ' }, 2);
            return parts.Length > 1 ? parts[1] : null;
        }
    }

    public class CreateSectionSubjectRequest
    {
        public int SectionId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public string? Schedule { get; set; }
        public string? Room { get; set; }
        public int? MaxStudents { get; set; }
    }

    public class UpdateSectionSubjectRequest
    {
        public int? TeacherId { get; set; }
        public string? Schedule { get; set; }
        public string? Room { get; set; }
        public int? MaxStudents { get; set; }
        public bool? IsActive { get; set; }
    }
}
