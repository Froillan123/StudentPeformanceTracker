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
                g.GradePoint,
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
                g.GradePoint,
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
                grade.GradePoint,
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
            // Validate AssessmentType is only Midterm or Final Grade
            if (request.AssessmentType != "Midterm" && request.AssessmentType != "Final Grade")
            {
                return BadRequest(new { message = "AssessmentType must be either 'Midterm' or 'Final Grade'" });
            }

            // Validate grade point range
            if (request.GradePoint < 1.0m || request.GradePoint > 5.0m)
            {
                return BadRequest(new { message = "Grade point must be between 1.0 and 5.0" });
            }

            // Validate student subject exists
            var studentSubject = await _studentSubjectRepository.GetByIdAsync(request.StudentSubjectId);
            if (studentSubject == null)
            {
                return NotFound(new { message = "Student subject enrollment not found" });
            }

            // Auto-fill AssessmentName if not provided
            var assessmentName = string.IsNullOrWhiteSpace(request.AssessmentName) 
                ? (request.AssessmentType == "Midterm" ? "Midterm Grade" : "Final Grade")
                : request.AssessmentName;

            var grade = new Grade
            {
                StudentSubjectId = request.StudentSubjectId,
                AssessmentType = request.AssessmentType,
                AssessmentName = assessmentName,
                GradePoint = request.GradePoint
                // DateGiven and Remarks are auto-generated in repository
            };

            var createdGrade = await _gradeRepository.CreateAsync(grade);

            var result = new
            {
                createdGrade.Id,
                createdGrade.StudentSubjectId,
                createdGrade.AssessmentType,
                createdGrade.AssessmentName,
                createdGrade.GradePoint,
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
            // Validate AssessmentType is only Midterm or Final Grade
            if (request.AssessmentType != "Midterm" && request.AssessmentType != "Final Grade")
            {
                return BadRequest(new { message = "AssessmentType must be either 'Midterm' or 'Final Grade'" });
            }

            // Validate grade point range
            if (request.GradePoint < 1.0m || request.GradePoint > 5.0m)
            {
                return BadRequest(new { message = "Grade point must be between 1.0 and 5.0" });
            }

            var existingGrade = await _gradeRepository.GetByIdAsync(id);
            if (existingGrade == null)
            {
                return NotFound(new { message = "Grade not found" });
            }

            // Auto-fill AssessmentName if not provided
            var assessmentName = string.IsNullOrWhiteSpace(request.AssessmentName) 
                ? (request.AssessmentType == "Midterm" ? "Midterm Grade" : "Final Grade")
                : request.AssessmentName;

            existingGrade.AssessmentType = request.AssessmentType;
            existingGrade.AssessmentName = assessmentName;
            existingGrade.GradePoint = request.GradePoint;
            // DateGiven and Remarks are auto-generated in repository

            var updatedGrade = await _gradeRepository.UpdateAsync(existingGrade);

            var result = new
            {
                updatedGrade.Id,
                updatedGrade.StudentSubjectId,
                updatedGrade.AssessmentType,
                updatedGrade.AssessmentName,
                updatedGrade.GradePoint,
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
    public string AssessmentType { get; set; } = string.Empty; // Only "Midterm" or "Final Grade"
    public string? AssessmentName { get; set; } // Optional, auto-filled if not provided
    public decimal GradePoint { get; set; } // 1.0 to 5.0
    // DateGiven and Remarks are auto-generated
}

public class UpdateGradeRequest
{
    public string AssessmentType { get; set; } = string.Empty; // Only "Midterm" or "Final Grade"
    public string? AssessmentName { get; set; } // Optional, auto-filled if not provided
    public decimal GradePoint { get; set; } // 1.0 to 5.0
    // DateGiven and Remarks are auto-generated
}

