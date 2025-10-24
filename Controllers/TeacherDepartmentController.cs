using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;
using StudentPeformanceTracker.Data;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teacher-department")]
[Authorize(Policy = "AdminOnly")]
public class TeacherDepartmentController : ControllerBase
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly AppDbContext _context;

    public TeacherDepartmentController(ITeacherRepository teacherRepository, IDepartmentRepository departmentRepository, AppDbContext context)
    {
        _teacherRepository = teacherRepository;
        _departmentRepository = departmentRepository;
        _context = context;
    }

    [HttpGet("teacher/{teacherId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetDepartmentsByTeacher(int teacherId)
    {
        try
        {
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var departments = teacher.TeacherDepartments.Select(td => new
            {
                td.DepartmentId,
                DepartmentName = td.Department.DepartmentName,
                DepartmentCode = td.Department.DepartmentCode,
                td.CreatedAt
            });

            return Ok(departments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving teacher departments", error = ex.Message });
        }
    }

    [HttpGet("department/{departmentId}")]
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
                t.HireDate,
                t.CreatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving department teachers", error = ex.Message });
        }
    }

    [HttpPost("assign")]
    public async Task<ActionResult> AssignTeacherToDepartment([FromBody] AssignTeacherRequest request)
    {
        try
        {
            // Validate teacher exists
            var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Validate department exists
            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
            if (department == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            // Check if assignment already exists
            var existingAssignment = teacher.TeacherDepartments
                .FirstOrDefault(td => td.DepartmentId == request.DepartmentId);
            
            if (existingAssignment != null)
            {
                return Conflict(new { message = "Teacher is already assigned to this department" });
            }

            // Create new assignment
            var teacherDepartment = new TeacherDepartment
            {
                TeacherId = request.TeacherId,
                DepartmentId = request.DepartmentId
            };

            // Add the assignment directly to the TeacherDepartments DbSet
            _context.TeacherDepartments.Add(teacherDepartment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Teacher assigned to department successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error assigning teacher to department", error = ex.Message });
        }
    }

    [HttpPost("unassign")]
    public async Task<ActionResult> UnassignTeacherFromDepartment([FromBody] UnassignTeacherRequest request)
    {
        try
        {
            // Validate teacher exists
            var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Find and remove assignment
            var assignment = teacher.TeacherDepartments
                .FirstOrDefault(td => td.DepartmentId == request.DepartmentId);
            
            if (assignment == null)
            {
                return NotFound(new { message = "Teacher is not assigned to this department" });
            }

            _context.TeacherDepartments.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Teacher unassigned from department successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error unassigning teacher from department", error = ex.Message });
        }
    }

    [HttpGet("unassigned-teachers")]
    public async Task<ActionResult<IEnumerable<object>>> GetUnassignedTeachers()
    {
        try
        {
            var allTeachers = await _teacherRepository.GetAllAsync();
            var unassignedTeachers = allTeachers
                .Where(t => !t.TeacherDepartments.Any())
                .Select(t => new
                {
                    t.Id,
                    t.UserId,
                    t.Email,
                    t.FirstName,
                    t.LastName,
                    t.Phone,
                    t.HireDate,
                    t.CreatedAt
                });

            return Ok(unassignedTeachers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving unassigned teachers", error = ex.Message });
        }
    }
}

public class AssignTeacherRequest
{
    public int TeacherId { get; set; }
    public int DepartmentId { get; set; }
}

public class UnassignTeacherRequest
{
    public int TeacherId { get; set; }
    public int DepartmentId { get; set; }
}
