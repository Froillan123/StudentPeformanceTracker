using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers
{
    [ApiController]
    [Route("api/v1/enrollment")]
    public class EnrollmentController : ControllerBase
    {
        private readonly EnrollmentService _enrollmentService;
        private readonly CourseSubjectService _courseSubjectService;

        public EnrollmentController(EnrollmentService enrollmentService, CourseSubjectService courseSubjectService)
        {
            _enrollmentService = enrollmentService;
            _courseSubjectService = courseSubjectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetAll()
        {
            var enrollments = await _enrollmentService.GetAllAsync();
            return Ok(enrollments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EnrollmentDto>> GetById(int id)
        {
            var enrollment = await _enrollmentService.GetByIdAsync(id);
            if (enrollment == null)
                return NotFound();

            return Ok(enrollment);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByStudentId(int studentId)
        {
            var enrollments = await _enrollmentService.GetByStudentIdAsync(studentId);
            return Ok(enrollments);
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByCourseId(int courseId)
        {
            var enrollments = await _enrollmentService.GetByCourseIdAsync(courseId);
            return Ok(enrollments);
        }

        [HttpPost]
        public async Task<ActionResult<EnrollmentDto>> Create(EnrollmentCreateRequest request)
        {
            try
            {
                var enrollment = await _enrollmentService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Enrollment enrollment)
        {
            if (id != enrollment.Id)
                return BadRequest();

            var updated = await _enrollmentService.UpdateAsync(enrollment);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _enrollmentService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/subjects")]
        public async Task<ActionResult<IEnumerable<StudentSubject>>> GetStudentSubjects(int id)
        {
            var studentSubjects = await _enrollmentService.GetStudentSubjectsAsync(id);
            return Ok(studentSubjects);
        }

        [HttpGet("available-subjects")]
        public async Task<ActionResult> GetAvailableSubjects([FromQuery] int courseId, [FromQuery] int yearLevelId, [FromQuery] int semesterId)
        {
            try
            {
                var subjects = await _courseSubjectService.GetByCourseYearSemesterAsync(courseId, yearLevelId, semesterId);
                return Ok(subjects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving available subjects", error = ex.Message });
            }
        }
    }
}
