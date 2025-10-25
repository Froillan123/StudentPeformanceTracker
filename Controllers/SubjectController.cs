using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subject")]
public class SubjectController : ControllerBase
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectController(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetSubjects([FromQuery] int? courseId = null)
    {
        try
        {
            IEnumerable<Subject> subjects;
            
            if (courseId.HasValue)
            {
                subjects = await _subjectRepository.GetByCourseIdAsync(courseId.Value);
            }
            else
            {
                subjects = await _subjectRepository.GetAllAsync();
            }
            
            var result = subjects.Select(s => new
            {
                s.Id,
                s.SubjectName,
                s.Description,
                s.Units,
                s.Prerequisites,
                s.IsActive,
                s.CourseId,
                CourseName = s.Course?.CourseName,
                s.CreatedAt,
                s.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving subjects", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSubject(int id)
    {
        try
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                return NotFound(new { message = "Subject not found" });
            }

            var result = new
            {
                subject.Id,
                subject.SubjectName,
                subject.Description,
                subject.Units,
                subject.Prerequisites,
                subject.IsActive,
                subject.CourseId,
                CourseName = subject.Course?.CourseName,
                subject.CreatedAt,
                subject.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving subject", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> CreateSubject([FromBody] CreateSubjectRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SubjectName))
            {
                return BadRequest(new { message = "Subject name is required" });
            }

            var subject = new Subject
            {
                SubjectName = request.SubjectName,
                Description = request.Description,
                Units = request.Units,
                Prerequisites = request.Prerequisites,
                IsActive = request.IsActive ?? true,
                CourseId = request.CourseId
            };

            var createdSubject = await _subjectRepository.CreateAsync(subject);

            var result = new
            {
                createdSubject.Id,
                createdSubject.SubjectName,
                createdSubject.Description,
                createdSubject.Units,
                createdSubject.Prerequisites,
                createdSubject.IsActive,
                createdSubject.CourseId,
                CourseName = createdSubject.Course?.CourseName,
                createdSubject.CreatedAt,
                createdSubject.UpdatedAt
            };

            return CreatedAtAction(nameof(GetSubject), new { id = createdSubject.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating subject", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectRequest request)
    {
        try
        {
            var existingSubject = await _subjectRepository.GetByIdAsync(id);
            if (existingSubject == null)
            {
                return NotFound(new { message = "Subject not found" });
            }

            if (string.IsNullOrWhiteSpace(request.SubjectName))
            {
                return BadRequest(new { message = "Subject name is required" });
            }

            existingSubject.SubjectName = request.SubjectName;
            existingSubject.Description = request.Description;
            existingSubject.Units = request.Units;
            existingSubject.Prerequisites = request.Prerequisites;
            existingSubject.IsActive = request.IsActive ?? existingSubject.IsActive;
            existingSubject.CourseId = request.CourseId;

            var updatedSubject = await _subjectRepository.UpdateAsync(existingSubject);

            var result = new
            {
                updatedSubject.Id,
                updatedSubject.SubjectName,
                updatedSubject.Description,
                updatedSubject.Units,
                updatedSubject.Prerequisites,
                updatedSubject.IsActive,
                updatedSubject.CourseId,
                CourseName = updatedSubject.Course?.CourseName,
                updatedSubject.CreatedAt,
                updatedSubject.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating subject", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteSubject(int id)
    {
        try
        {
            var deleted = await _subjectRepository.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Subject not found" });
            }

            return Ok(new { message = "Subject deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting subject", error = ex.Message });
        }
    }
}

public class CreateSubjectRequest
{
    public string SubjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Units { get; set; } = 3;
    public string? Prerequisites { get; set; }
    public bool? IsActive { get; set; }
    public int? CourseId { get; set; }
}

public class UpdateSubjectRequest
{
    public string SubjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Units { get; set; }
    public string? Prerequisites { get; set; }
    public bool? IsActive { get; set; }
    public int? CourseId { get; set; }
}