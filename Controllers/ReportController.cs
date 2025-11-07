using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/report")]
public class ReportController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("student/{studentId}/grades/pdf")]
    public async Task<IActionResult> GenerateStudentGradeReport(int studentId)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateStudentGradeReportPdf(studentId);
            var fileName = $"GradeReport_Student{studentId}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating report", error = ex.Message });
        }
    }

    [HttpGet("student/{studentId}/grades/excel")]
    public async Task<IActionResult> ExportStudentGradesExcel(int studentId)
    {
        try
        {
            var excelBytes = await _reportService.ExportStudentGradesToExcel(studentId);
            var fileName = $"Grades_Student{studentId}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error exporting grades", error = ex.Message });
        }
    }

    [HttpGet("admin/analytics/pdf")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GenerateAnalyticsReport()
    {
        try
        {
            var pdfBytes = await _reportService.GenerateAnalyticsReportPdf();
            var fileName = $"AnalyticsReport_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating analytics report", error = ex.Message });
        }
    }

    [HttpGet("admin/analytics/excel")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ExportAnalyticsExcel()
    {
        try
        {
            var excelBytes = await _reportService.ExportAnalyticsToExcel();
            var fileName = $"AnalyticsReport_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error exporting analytics report", error = ex.Message });
        }
    }

    [HttpGet("teacher/class/{sectionSubjectId}/pdf")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<IActionResult> GenerateClassGradeReport(int sectionSubjectId)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateClassGradeReportPdf(sectionSubjectId);
            var fileName = $"ClassReport_SectionSubject{sectionSubjectId}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating class report", error = ex.Message });
        }
    }

    [HttpGet("teacher/class/{sectionSubjectId}/excel")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<IActionResult> ExportClassGradesExcel(int sectionSubjectId)
    {
        try
        {
            var excelBytes = await _reportService.ExportClassGradesToExcel(sectionSubjectId);
            var fileName = $"ClassGrades_SectionSubject{sectionSubjectId}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error exporting class grades", error = ex.Message });
        }
    }

    [HttpGet("teacher/{teacherId}/analytics/pdf")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<IActionResult> GenerateTeacherAnalyticsReport(int teacherId)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateTeacherAnalyticsReportPdf(teacherId);
            var fileName = $"TeacherAnalyticsReport_{teacherId}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating teacher analytics report", error = ex.Message });
        }
    }

    [HttpGet("teacher/{teacherId}/analytics/excel")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<IActionResult> ExportTeacherAnalyticsExcel(int teacherId)
    {
        try
        {
            var excelBytes = await _reportService.ExportTeacherAnalyticsToExcel(teacherId);
            var fileName = $"TeacherAnalyticsReport_{teacherId}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error exporting teacher analytics report", error = ex.Message });
        }
    }
}

