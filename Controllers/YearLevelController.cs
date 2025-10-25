using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Models;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/yearlevel")]
public class YearLevelController : ControllerBase
{
    private readonly IYearLevelRepository _yearLevelRepository;

    public YearLevelController(IYearLevelRepository yearLevelRepository)
    {
        _yearLevelRepository = yearLevelRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetYearLevels()
    {
        try
        {
            var yearLevels = await _yearLevelRepository.GetAllAsync();
            var result = yearLevels.Select(yl => new
            {
                yl.Id,
                yl.LevelNumber,
                yl.LevelName,
                yl.Description,
                yl.CreatedAt,
                yl.UpdatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving year levels", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetYearLevel(int id)
    {
        try
        {
            var yearLevel = await _yearLevelRepository.GetByIdAsync(id);
            if (yearLevel == null)
            {
                return NotFound(new { message = "Year level not found" });
            }

            var result = new
            {
                yearLevel.Id,
                yearLevel.LevelNumber,
                yearLevel.LevelName,
                yearLevel.Description,
                yearLevel.CreatedAt,
                yearLevel.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving year level", error = ex.Message });
        }
    }
}