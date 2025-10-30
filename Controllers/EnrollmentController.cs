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
        private readonly Repository.Interfaces.IStudentSubjectRepository _studentSubjectRepository;
        private readonly Repository.Interfaces.ISectionSubjectRepository _sectionSubjectRepository;
        private readonly Repository.Interfaces.IEnrollmentRepository _enrollmentRepository;

        public EnrollmentController(EnrollmentService enrollmentService, CourseSubjectService courseSubjectService,
            Repository.Interfaces.IStudentSubjectRepository studentSubjectRepository,
            Repository.Interfaces.ISectionSubjectRepository sectionSubjectRepository,
            Repository.Interfaces.IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentService = enrollmentService;
            _courseSubjectService = courseSubjectService;
            _studentSubjectRepository = studentSubjectRepository;
            _sectionSubjectRepository = sectionSubjectRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var enrollments = await _enrollmentService.GetAllAsync();
            var totalCount = enrollments.Count();
            var paginatedEnrollments = enrollments.Skip((page - 1) * pageSize).Take(pageSize);
            
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var paginatedResult = new PaginatedResult<object>
            {
                Data = paginatedEnrollments,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(paginatedResult);
        }

        [HttpGet("{enrollmentId}/pending")]
        public async Task<ActionResult<object>> GetPendingByEnrollment(int enrollmentId)
        {
            var items = await _studentSubjectRepository.GetByEnrollmentIdAsync(enrollmentId);
            var pending = items.Where(ss => ss.Status == "Pending");
            var list = pending.Select(ss => new
            {
                ss.Id,
                ss.StudentId,
                ss.SectionSubjectId,
                SubjectName = ss.SectionSubject?.Subject?.SubjectName,
                Units = ss.SectionSubject?.Subject?.Units ?? 0,
                SectionName = ss.SectionSubject?.Section?.SectionName,
                ss.Status
            }).ToList();
            var totalUnits = list.Sum(x => x.Units);
            return Ok(new { totalUnits, data = list });
        }

        [HttpPost("{enrollmentId}/approve")]
        public async Task<ActionResult> ApprovePending(int enrollmentId)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null) return NotFound(new { message = "Enrollment not found" });

            var items = await _studentSubjectRepository.GetByEnrollmentIdAsync(enrollmentId);
            var pending = items.Where(ss => ss.Status == "Pending").ToList();

            foreach (var ss in pending)
            {
                var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(ss.SectionSubjectId);
                if (sectionSubject == null) continue;
                if (sectionSubject.CurrentEnrollment > sectionSubject.MaxStudents)
                {
                    return BadRequest(new { message = $"Class {sectionSubject.Subject?.SubjectName} is already full." });
                }
                // Promote to Enrolled (capacity was already incremented during pending selection)
                ss.Status = "Enrolled";
                ss.UpdatedAt = DateTime.UtcNow;
                await _studentSubjectRepository.UpdateAsync(ss);
            }

            if (enrollment.Status == "Pending")
            {
                enrollment.Status = "Active";
                enrollment.UpdatedAt = DateTime.UtcNow;
                await _enrollmentRepository.UpdateAsync(enrollment);
            }

            return Ok(new { message = "Enrollment approved.", approved = pending.Count });
        }

        [HttpPost("{enrollmentId}/reject")]
        public async Task<ActionResult> RejectPending(int enrollmentId)
        {
            var items = await _studentSubjectRepository.GetByEnrollmentIdAsync(enrollmentId);
            var pending = items.Where(ss => ss.Status == "Pending").ToList();
            foreach (var ss in pending)
            {
                // Decrement capacity to free up the reserved slot
                var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(ss.SectionSubjectId);
                if (sectionSubject != null)
                {
                    sectionSubject.CurrentEnrollment--;
                    sectionSubject.UpdatedAt = DateTime.UtcNow;
                    await _sectionSubjectRepository.UpdateAsync(sectionSubject);
                }
                await _studentSubjectRepository.DeleteAsync(ss.Id);
            }
            return Ok(new { message = "Pending selections rejected.", removed = pending.Count });
        }

        public class UpdateEnrollmentStatusRequest { public string Status { get; set; } = "Active"; }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> PatchStatus(int id, [FromBody] UpdateEnrollmentStatusRequest request)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(id);
            if (enrollment == null) return NotFound(new { message = "Enrollment not found" });
            enrollment.Status = request.Status;
            enrollment.UpdatedAt = DateTime.UtcNow;
            await _enrollmentRepository.UpdateAsync(enrollment);
            return Ok(new { message = "Status updated", enrollmentId = id, status = enrollment.Status });
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
