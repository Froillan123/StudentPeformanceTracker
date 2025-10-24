using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public TeacherController(ITeacherRepository teacherRepository, IDepartmentRepository departmentRepository)
    {
        _teacherRepository = teacherRepository;
        _departmentRepository = departmentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTeachers()
    {
        try
        {
            var teachers = await _teacherRepository.GetAllAsync();
            var result = teachers.Select(t => new
            {
                t.Id,
                t.UserId,
                t.Email,
                t.FirstName,
                t.LastName,
                t.Phone,
                t.DepartmentId,
                DepartmentName = t.Department?.DepartmentName,
                t.HireDate,
                t.CreatedAt,
                t.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teachers", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTeacher(int id)
    {
        try
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var result = new
            {
                teacher.Id,
                teacher.UserId,
                teacher.Email,
                teacher.FirstName,
                teacher.LastName,
                teacher.Phone,
                teacher.DepartmentId,
                DepartmentName = teacher.Department?.DepartmentName,
                teacher.HireDate,
                teacher.CreatedAt,
                teacher.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teacher", error = ex.Message });
        }
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateTeacher(int id, [FromBody] UpdateTeacherRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTeacher = await _teacherRepository.GetByIdAsync(id);
            if (existingTeacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Check if email already exists (excluding current teacher)
            if (request.Email != existingTeacher.Email &&
                await _teacherRepository.EmailExistsAsync(request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Validate department exists if provided
            if (request.DepartmentId.HasValue)
            {
                if (!await _departmentRepository.ExistsAsync(request.DepartmentId.Value))
                {
                    return BadRequest(new { message = "Department not found" });
                }
            }

            existingTeacher.Email = request.Email;
            existingTeacher.FirstName = request.FirstName;
            existingTeacher.LastName = request.LastName;
            existingTeacher.Phone = request.Phone;
            existingTeacher.DepartmentId = request.DepartmentId;
            existingTeacher.HireDate = request.HireDate;

            var updatedTeacher = await _teacherRepository.UpdateAsync(existingTeacher);

            var result = new
            {
                updatedTeacher.Id,
                updatedTeacher.UserId,
                updatedTeacher.Email,
                updatedTeacher.FirstName,
                updatedTeacher.LastName,
                updatedTeacher.Phone,
                updatedTeacher.DepartmentId,
                DepartmentName = updatedTeacher.Department?.DepartmentName,
                updatedTeacher.HireDate,
                updatedTeacher.CreatedAt,
                updatedTeacher.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating teacher", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTeacher(int id)
    {
        try
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var deleted = await _teacherRepository.DeleteAsync(id);
            if (!deleted)
            {
                return StatusCode(500, new { message = "Failed to delete teacher" });
            }

            return Ok(new { message = "Teacher deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting teacher", error = ex.Message });
        }
    }

    [HttpGet("by-department/{departmentId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetTeachersByDepartment(int departmentId)
    {
        try
        {
            var teachers = await _teacherRepository.GetByDepartmentIdAsync(departmentId);
            var result = teachers.Select(t => new
            {
                t.Id,
                t.UserId,
                t.Email,
                t.FirstName,
                t.LastName,
                t.Phone,
                t.DepartmentId,
                DepartmentName = t.Department?.DepartmentName,
                t.HireDate,
                t.CreatedAt,
                t.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teachers by department", error = ex.Message });
        }
    }
}

public class CreateTeacherRequest
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? HireDate { get; set; }
}

public class UpdateTeacherRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? HireDate { get; set; }
}
