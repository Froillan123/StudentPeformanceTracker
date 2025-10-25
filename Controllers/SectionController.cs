using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.DTO;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SectionController : ControllerBase
    {
        private readonly SectionService _sectionService;

        public SectionController(SectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetAll()
        {
            var sections = await _sectionService.GetAllAsync();
            return Ok(sections);
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
        public async Task<ActionResult<SectionDto>> Create(Section section)
        {
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
        public async Task<ActionResult<IEnumerable<SectionSubject>>> GetSectionSubjects(int id)
        {
            var sectionSubjects = await _sectionService.GetSectionSubjectsAsync(id);
            return Ok(sectionSubjects);
        }
    }
}
