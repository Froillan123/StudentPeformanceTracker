using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers
{
    [ApiController]
    [Route("api/v1/semester")]
    public class SemesterController : ControllerBase
    {
        private readonly ISemesterRepository _semesterRepository;

        public SemesterController(ISemesterRepository semesterRepository)
        {
            _semesterRepository = semesterRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Semester>>> GetAll()
        {
            var semesters = await _semesterRepository.GetAllAsync();
            return Ok(semesters);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Semester>>> GetActive()
        {
            var semesters = await _semesterRepository.GetActiveAsync();
            return Ok(semesters);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Semester>> GetById(int id)
        {
            var semester = await _semesterRepository.GetByIdAsync(id);
            if (semester == null)
                return NotFound();

            return Ok(semester);
        }

        [HttpGet("code/{semesterCode}")]
        public async Task<ActionResult<Semester>> GetBySemesterCode(string semesterCode)
        {
            var semester = await _semesterRepository.GetBySemesterCodeAsync(semesterCode);
            if (semester == null)
                return NotFound();

            return Ok(semester);
        }

        [HttpPost]
        public async Task<ActionResult<Semester>> Create(Semester semester)
        {
            var created = await _semesterRepository.CreateAsync(semester);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Semester semester)
        {
            if (id != semester.Id)
                return BadRequest();

            var updated = await _semesterRepository.UpdateAsync(semester);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _semesterRepository.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
