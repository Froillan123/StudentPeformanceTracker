using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers
{
    [ApiController]
    [Route("api/v1/coursesubject")]
    public class CourseSubjectController : ControllerBase
    {
        private readonly CourseSubjectService _courseSubjectService;

        public CourseSubjectController(CourseSubjectService courseSubjectService)
        {
            _courseSubjectService = courseSubjectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseSubjectDto>>> GetAll()
        {
            var courseSubjects = await _courseSubjectService.GetAllAsync();
            return Ok(courseSubjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseSubjectDto>> GetById(int id)
        {
            var courseSubject = await _courseSubjectService.GetByIdAsync(id);
            if (courseSubject == null)
                return NotFound();

            return Ok(courseSubject);
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<CourseSubjectDto>>> GetByCourseId(int courseId)
        {
            var courseSubjects = await _courseSubjectService.GetByCourseIdAsync(courseId);
            return Ok(courseSubjects);
        }

        [HttpGet("course/{courseId}/year/{yearLevelId}")]
        public async Task<ActionResult<IEnumerable<CourseSubjectDto>>> GetByCourseAndYear(int courseId, int yearLevelId)
        {
            var courseSubjects = await _courseSubjectService.GetByCourseAndYearAsync(courseId, yearLevelId);
            return Ok(courseSubjects);
        }

        [HttpGet("course/{courseId}/year/{yearLevelId}/semester/{semesterId}")]
        public async Task<ActionResult<IEnumerable<CourseSubjectDto>>> GetByCourseYearSemester(int courseId, int yearLevelId, int semesterId)
        {
            var courseSubjects = await _courseSubjectService.GetByCourseYearSemesterAsync(courseId, yearLevelId, semesterId);
            return Ok(courseSubjects);
        }

        [HttpPost]
        public async Task<ActionResult<CourseSubjectDto>> Create([FromBody] CreateCourseSubjectRequest request)
        {
            try
            {
                var courseSubject = await _courseSubjectService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = courseSubject.Id }, courseSubject);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating course subject", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _courseSubjectService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Course subject not found" });

                return Ok(new { message = "Course subject deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting course subject", error = ex.Message });
            }
        }
    }
}
