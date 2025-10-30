using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/course")]
public class CourseController : ControllerBase
{
    private readonly ICourseRepository _courseRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public CourseController(ICourseRepository courseRepository, IDepartmentRepository departmentRepository)
    {
        _courseRepository = courseRepository;
        _departmentRepository = departmentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var (courses, totalCount) = await _courseRepository.GetPaginatedAsync(page, pageSize);
            var result = courses.Select(c => new
            {
                c.Id,
                c.CourseName,
                c.Description,
                c.DepartmentId,
                DepartmentName = c.Department?.DepartmentName,
                c.CreatedAt,
                c.UpdatedAt
            });

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var paginatedResult = new PaginatedResult<object>
            {
                Data = result,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(paginatedResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving courses", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetCourse(int id)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }

            var result = new
            {
                course.Id,
                course.CourseName,
                course.Description,
                course.DepartmentId,
                DepartmentName = course.Department?.DepartmentName,
                course.CreatedAt,
                course.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving course", error = ex.Message });
        }
    }

    [HttpGet("department/{departmentId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetCoursesByDepartment(int departmentId)
    {
        try
        {
            var courses = await _courseRepository.GetByDepartmentIdAsync(departmentId);
            var result = courses.Select(c => new
            {
                c.Id,
                c.CourseName,
                c.Description,
                c.DepartmentId,
                DepartmentName = c.Department?.DepartmentName,
                c.CreatedAt,
                c.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving courses by department", error = ex.Message });
        }
    }

    [HttpGet("{id}/subjects")]
    public async Task<ActionResult<IEnumerable<object>>> GetCourseSubjects(int id)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }

            // Get course subjects with related data
            var courseSubjects = course.CourseSubjects?.Select(cs => new
            {
                cs.Id,
                SubjectId = cs.SubjectId,
                SubjectName = cs.Subject?.SubjectName,
                Units = cs.Subject?.Units,
                YearLevelId = cs.YearLevelId,
                YearLevelName = cs.YearLevel?.LevelName,
                SemesterId = cs.SemesterId,
                SemesterName = cs.Semester?.SemesterName,
                SchoolYear = cs.Semester?.SchoolYear,
                IsRequired = cs.IsRequired,
                cs.CreatedAt,
                cs.UpdatedAt
            }) ?? Enumerable.Empty<object>();

            return Ok(courseSubjects);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving course subjects", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CourseName))
            {
                return BadRequest(new { message = "Course name is required" });
            }

            // Check if course name already exists
            if (await _courseRepository.CourseNameExistsAsync(request.CourseName))
            {
                return Conflict(new { message = "Course with this name already exists" });
            }

            // Validate department exists if provided
            if (request.DepartmentId.HasValue && !await _departmentRepository.ExistsAsync(request.DepartmentId.Value))
            {
                return BadRequest(new { message = "Department not found" });
            }

            var course = new Course
            {
                CourseName = request.CourseName,
                Description = request.Description,
                DepartmentId = request.DepartmentId
            };

            var createdCourse = await _courseRepository.CreateAsync(course);

            var result = new
            {
                createdCourse.Id,
                createdCourse.CourseName,
                createdCourse.Description,
                createdCourse.DepartmentId,
                DepartmentName = createdCourse.Department?.DepartmentName,
                createdCourse.CreatedAt,
                createdCourse.UpdatedAt
            };

            return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating course", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<object>> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        try
        {
            var existingCourse = await _courseRepository.GetByIdAsync(id);
            if (existingCourse == null)
            {
                return NotFound(new { message = "Course not found" });
            }

            if (string.IsNullOrWhiteSpace(request.CourseName))
            {
                return BadRequest(new { message = "Course name is required" });
            }

            // Check if course name already exists (excluding current course)
            if (request.CourseName != existingCourse.CourseName &&
                await _courseRepository.CourseNameExistsAsync(request.CourseName))
            {
                return Conflict(new { message = "Course with this name already exists" });
            }

            // Validate department exists if provided
            if (request.DepartmentId.HasValue && !await _departmentRepository.ExistsAsync(request.DepartmentId.Value))
            {
                return BadRequest(new { message = "Department not found" });
            }

            existingCourse.CourseName = request.CourseName;
            existingCourse.Description = request.Description;
            existingCourse.DepartmentId = request.DepartmentId;

            var updatedCourse = await _courseRepository.UpdateAsync(existingCourse);

            var result = new
            {
                updatedCourse.Id,
                updatedCourse.CourseName,
                updatedCourse.Description,
                updatedCourse.DepartmentId,
                DepartmentName = updatedCourse.Department?.DepartmentName,
                updatedCourse.CreatedAt,
                updatedCourse.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating course", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        try
        {
            var deleted = await _courseRepository.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Course not found" });
            }

            return Ok(new { message = "Course deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting course", error = ex.Message });
        }
    }
}

public class CreateCourseRequest
{
    public string CourseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
}

public class UpdateCourseRequest
{
    public string CourseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
}
