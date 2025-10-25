using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/grade")]
public class GradeController : ControllerBase
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IStudentSubjectRepository _studentSubjectRepository;

    public GradeController(IGradeRepository gradeRepository, IStudentSubjectRepository studentSubjectRepository)
    {
        _gradeRepository = gradeRepository;
        _studentSubjectRepository = studentSubjectRepository;
    }

    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetStudentGrades(int studentId)
    {
        try
        {
            var grades = await _gradeRepository.GetByStudentIdAsync(studentId);
            var result = grades.Select(g => new
            {
                g.Id,
                g.StudentSubjectId,
                SubjectName = g.StudentSubject?.SectionSubject?.Subject?.SubjectName,
                SectionName = g.StudentSubject?.SectionSubject?.Section?.SectionName,
                g.AssessmentType,
                g.AssessmentName,
                g.Score,
                g.MaxScore,
                g.Percentage,
                g.Remarks,
                g.DateGiven,
                g.CreatedAt,
                g.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving student grades", error = ex.Message });
        }
    }

    [HttpGet("studentsubject/{studentSubjectId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetGradesByStudentSubject(int studentSubjectId)
    {
        try
        {
            var grades = await _gradeRepository.GetByStudentSubjectIdAsync(studentSubjectId);
            var result = grades.Select(g => new
            {
                g.Id,
                g.StudentSubjectId,
                SubjectName = g.StudentSubject?.SectionSubject?.Subject?.SubjectName,
                g.AssessmentType,
                g.AssessmentName,
                g.Score,
                g.MaxScore,
                g.Percentage,
                g.Remarks,
                g.DateGiven,
                g.CreatedAt,
                g.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving grades", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetGrade(int id)
    {
        try
        {
            var grade = await _gradeRepository.GetByIdAsync(id);
            if (grade == null)
            {
                return NotFound(new { message = "Grade not found" });
            }

            var result = new
            {
                grade.Id,
                grade.StudentSubjectId,
                StudentName = grade.StudentSubject?.Student != null 
                    ? $"{grade.StudentSubject.Student.FirstName} {grade.StudentSubject.Student.LastName}"
                    : null,
                SubjectName = grade.StudentSubject?.SectionSubject?.Subject?.SubjectName,
                grade.AssessmentType,
                grade.AssessmentName,
                grade.Score,
                grade.MaxScore,
                grade.Percentage,
                grade.Remarks,
                grade.DateGiven,
                grade.CreatedAt,
                grade.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving grade", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<object>> CreateGrade([FromBody] CreateGradeRequest request)
    {
        try
        {
            // Validate student subject exists
            var studentSubject = await _studentSubjectRepository.GetByIdAsync(request.StudentSubjectId);
            if (studentSubject == null)
            {
                return NotFound(new { message = "Student subject enrollment not found" });
            }

            if (request.Score < 0 || request.MaxScore <= 0 || request.Score > request.MaxScore)
            {
                return BadRequest(new { message = "Invalid score or max score" });
            }

            // Convert DateGiven to UTC if provided
            DateTime? dateGivenUtc = null;
            if (request.DateGiven.HasValue)
            {
                dateGivenUtc = request.DateGiven.Value.Kind == DateTimeKind.Utc 
                    ? request.DateGiven.Value 
                    : DateTime.SpecifyKind(request.DateGiven.Value, DateTimeKind.Utc);
            }

            var grade = new Grade
            {
                StudentSubjectId = request.StudentSubjectId,
                AssessmentType = request.AssessmentType,
                AssessmentName = request.AssessmentName,
                Score = request.Score,
                MaxScore = request.MaxScore,
                Remarks = request.Remarks,
                DateGiven = dateGivenUtc ?? DateTime.UtcNow
            };

            var createdGrade = await _gradeRepository.CreateAsync(grade);

            var result = new
            {
                createdGrade.Id,
                createdGrade.StudentSubjectId,
                createdGrade.AssessmentType,
                createdGrade.AssessmentName,
                createdGrade.Score,
                createdGrade.MaxScore,
                createdGrade.Percentage,
                createdGrade.Remarks,
                createdGrade.DateGiven,
                createdGrade.CreatedAt,
                createdGrade.UpdatedAt
            };

            return CreatedAtAction(nameof(GetGrade), new { id = createdGrade.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating grade", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<object>> UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
    {
        try
        {
            var existingGrade = await _gradeRepository.GetByIdAsync(id);
            if (existingGrade == null)
            {
                return NotFound(new { message = "Grade not found" });
            }

            if (request.Score < 0 || request.MaxScore <= 0 || request.Score > request.MaxScore)
            {
                return BadRequest(new { message = "Invalid score or max score" });
            }

            existingGrade.AssessmentType = request.AssessmentType;
            existingGrade.AssessmentName = request.AssessmentName;
            existingGrade.Score = request.Score;
            existingGrade.MaxScore = request.MaxScore;
            existingGrade.Remarks = request.Remarks;
            
            // Convert DateGiven to UTC if provided
            if (request.DateGiven.HasValue)
            {
                existingGrade.DateGiven = request.DateGiven.Value.Kind == DateTimeKind.Utc 
                    ? request.DateGiven.Value 
                    : DateTime.SpecifyKind(request.DateGiven.Value, DateTimeKind.Utc);
            }

            var updatedGrade = await _gradeRepository.UpdateAsync(existingGrade);

            var result = new
            {
                updatedGrade.Id,
                updatedGrade.StudentSubjectId,
                updatedGrade.AssessmentType,
                updatedGrade.AssessmentName,
                updatedGrade.Score,
                updatedGrade.MaxScore,
                updatedGrade.Percentage,
                updatedGrade.Remarks,
                updatedGrade.DateGiven,
                updatedGrade.CreatedAt,
                updatedGrade.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating grade", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult> DeleteGrade(int id)
    {
        try
        {
            var deleted = await _gradeRepository.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Grade not found" });
            }

            return Ok(new { message = "Grade deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting grade", error = ex.Message });
        }
    }
}

public class CreateGradeRequest
{
    public int StudentSubjectId { get; set; }
    public string AssessmentType { get; set; } = string.Empty; // Quiz, Exam, Project, Assignment
    public string AssessmentName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string? Remarks { get; set; }
    public DateTime? DateGiven { get; set; }
}

public class UpdateGradeRequest
{
    public string AssessmentType { get; set; } = string.Empty;
    public string AssessmentName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string? Remarks { get; set; }
    public DateTime? DateGiven { get; set; }
}

