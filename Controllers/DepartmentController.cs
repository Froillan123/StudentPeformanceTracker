using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/department")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentController(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetDepartments()
    {
        try
        {
            var departments = await _departmentRepository.GetAllAsync();
            var result = departments.Select(d => new
            {
                d.Id,
                d.DepartmentName,
                d.DepartmentCode,
                d.Description,
                d.HeadOfDepartment,
                d.CreatedAt,
                d.UpdatedAt,
                TeachersCount = d.TeacherDepartments?.Count ?? 0,
                // Debug info
                TeacherDepartmentsCount = d.TeacherDepartments?.Count ?? 0,
                TeacherDepartments = d.TeacherDepartments?.Select(td => new { td.TeacherId, td.DepartmentId }).ToList()
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving departments", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetDepartment(int id)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            var result = new
            {
                department.Id,
                department.DepartmentName,
                department.DepartmentCode,
                department.Description,
                department.HeadOfDepartment,
                department.CreatedAt,
                department.UpdatedAt,
                Teachers = department.TeacherDepartments?.Select(td => new
                {
                    td.Teacher.Id,
                    td.Teacher.FirstName,
                    td.Teacher.LastName,
                    td.Teacher.Email
                }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving department", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if department name already exists
            if (await _departmentRepository.ExistsByNameAsync(request.DepartmentName))
            {
                return BadRequest(new { message = "Department name already exists" });
            }

            // Check if department code already exists (if provided)
            if (!string.IsNullOrEmpty(request.DepartmentCode) && 
                await _departmentRepository.ExistsByCodeAsync(request.DepartmentCode))
            {
                return BadRequest(new { message = "Department code already exists" });
            }

            var department = new Department
            {
                DepartmentName = request.DepartmentName,
                DepartmentCode = request.DepartmentCode,
                Description = request.Description,
                HeadOfDepartment = request.HeadOfDepartment
            };

            var createdDepartment = await _departmentRepository.CreateAsync(department);

            var result = new
            {
                createdDepartment.Id,
                createdDepartment.DepartmentName,
                createdDepartment.DepartmentCode,
                createdDepartment.Description,
                createdDepartment.HeadOfDepartment,
                createdDepartment.CreatedAt,
                createdDepartment.UpdatedAt
            };

            return CreatedAtAction(nameof(GetDepartment), new { id = createdDepartment.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating department", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            // Check if department name already exists (excluding current department)
            if (request.DepartmentName != existingDepartment.DepartmentName &&
                await _departmentRepository.ExistsByNameAsync(request.DepartmentName))
            {
                return BadRequest(new { message = "Department name already exists" });
            }

            // Check if department code already exists (if provided and different)
            if (!string.IsNullOrEmpty(request.DepartmentCode) && 
                request.DepartmentCode != existingDepartment.DepartmentCode &&
                await _departmentRepository.ExistsByCodeAsync(request.DepartmentCode))
            {
                return BadRequest(new { message = "Department code already exists" });
            }

            existingDepartment.DepartmentName = request.DepartmentName;
            existingDepartment.DepartmentCode = request.DepartmentCode;
            existingDepartment.Description = request.Description;
            existingDepartment.HeadOfDepartment = request.HeadOfDepartment;

            var updatedDepartment = await _departmentRepository.UpdateAsync(existingDepartment);

            var result = new
            {
                updatedDepartment.Id,
                updatedDepartment.DepartmentName,
                updatedDepartment.DepartmentCode,
                updatedDepartment.Description,
                updatedDepartment.HeadOfDepartment,
                updatedDepartment.CreatedAt,
                updatedDepartment.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating department", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDepartment(int id)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            // Check if department has teachers
            if (department.TeacherDepartments?.Any() == true)
            {
                return BadRequest(new { message = "Cannot delete department with assigned teachers" });
            }

            var deleted = await _departmentRepository.DeleteAsync(id);
            if (!deleted)
            {
                return StatusCode(500, new { message = "Failed to delete department" });
            }

            return Ok(new { message = "Department deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting department", error = ex.Message });
        }
    }
}

public class CreateDepartmentRequest
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public string? Description { get; set; }
    public string? HeadOfDepartment { get; set; }
}

public class UpdateDepartmentRequest
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public string? Description { get; set; }
    public string? HeadOfDepartment { get; set; }
}
