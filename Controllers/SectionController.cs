using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers
{
    [ApiController]
    [Route("api/v1/section")]
    public class SectionController : ControllerBase
    {
        private readonly SectionService _sectionService;

        public SectionController(SectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var sections = await _sectionService.GetAllAsync();
            var totalCount = sections.Count();
            var paginatedSections = sections.Skip((page - 1) * pageSize).Take(pageSize);
            
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var paginatedResult = new PaginatedResult<object>
            {
                Data = paginatedSections,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(paginatedResult);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SectionDto>> GetById(int id)
        {
            var section = await _sectionService.GetByIdAsync(id);
            if (section == null)
                return NotFound();

            return Ok(section);
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetByCourseId(int courseId)
        {
            var sections = await _sectionService.GetByCourseIdAsync(courseId);
            return Ok(sections);
        }

        [HttpGet("course/{courseId}/year/{yearLevelId}/semester/{semesterId}")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetByCourseYearSemester(int courseId, int yearLevelId, int semesterId)
        {
            var sections = await _sectionService.GetByCourseYearSemesterAsync(courseId, yearLevelId, semesterId);
            return Ok(sections);
        }

        [HttpPost]
        public async Task<ActionResult<SectionDto>> Create(CreateSectionRequest request)
        {
            var section = new Section
            {
                SectionName = request.SectionName,
                CourseId = request.CourseId,
                YearLevelId = request.YearLevelId,
                SemesterId = request.SemesterId,
                MaxCapacity = request.MaxCapacity,
                CurrentEnrollment = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _sectionService.CreateAsync(section);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Section section)
        {
            if (id != section.Id)
                return BadRequest();

            var updated = await _sectionService.UpdateAsync(section);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _sectionService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/subjects")]
        public async Task<ActionResult<IEnumerable<object>>> GetSectionSubjects(int id)
        {
            var sectionSubjects = await _sectionService.GetSectionSubjectsAsync(id);
            var result = sectionSubjects.Select(ss => new
            {
                ss.Id,
                ss.SectionId,
                SectionName = ss.Section?.SectionName,
                ss.SubjectId,
                SubjectName = ss.Subject?.SubjectName,
                SubjectDescription = ss.Subject?.Description,
                ss.TeacherId,
                TeacherName = ss.Teacher != null ? $"{ss.Teacher.FirstName} {ss.Teacher.LastName}" : "Not assigned",
                ss.ScheduleDay,
                ss.ScheduleTime,
                Schedule = !string.IsNullOrEmpty(ss.ScheduleDay) && !string.IsNullOrEmpty(ss.ScheduleTime) 
                    ? $"{ss.ScheduleDay}: {ss.ScheduleTime}" 
                    : "TBA",
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
    }

    public class CreateSectionRequest
    {
        public string SectionName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int YearLevelId { get; set; }
        public int SemesterId { get; set; }
        public int MaxCapacity { get; set; }
    }
}
