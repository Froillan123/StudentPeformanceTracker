# Phases 4, 5, 6 Implementation Guide

## Phase 4: Teacher Grade Management - COMPLETED UPDATES

### ✅ Updated Pages/Teacher/MyClasses.cshtml
- Fetches real teacher assignments from `/api/v1/sectionsubject/teacher/{teacherId}`
- Displays class cards dynamically with:
  - Subject Name, Section Name, Schedule, EDP Code, Student Count
  - "Manage Grades" button linking to `/Teacher/Grades?sectionSubjectId={id}`
- Empty state when no classes assigned

### 🔄 Update Pages/Teacher/Grades.cshtml (MANUAL UPDATE REQUIRED)

The existing Grades.cshtml needs these JavaScript additions:

```javascript
// At the top of DOMContentLoaded
const urlParams = new URLSearchParams(window.location.search);
const sectionSubjectId = urlParams.get('sectionSubjectId');

if (sectionSubjectId) {
    loadSectionSubjectDetails(sectionSubjectId);
    loadStudentsInClass(sectionSubjectId);
}

// New functions to add:

async function loadSectionSubjectDetails(sectionSubjectId) {
    try {
        const response = await fetch(`/api/v1/sectionsubject/${sectionSubjectId}`);
        if (response.ok) {
            const details = await response.json();
            document.querySelector('.h3.mb-0').textContent = 
                `Grade Management - ${details.subjectName} (${details.sectionName})`;
        }
    } catch (error) {
        console.error('Error loading section details:', error);
    }
}

async function loadStudentsInClass(sectionSubjectId) {
    try {
        const response = await fetch(`/api/v1/sectionsubject/${sectionSubjectId}/students`);
        if (response.ok) {
            const students = await response.json();
            displayStudents(students, sectionSubjectId);
        }
    } catch (error) {
        console.error('Error loading students:', error);
    }
}

function displayStudents(students, sectionSubjectId) {
    const tbody = document.querySelector('tbody');
    tbody.innerHTML = '';
    
    students.forEach(student => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${student.firstName} ${student.lastName}</td>
            <td>${student.studentId}</td>
            <td id="grades-${student.studentSubjectId}">
                <button class="btn btn-sm btn-outline-secondary" onclick="viewGrades(${student.studentSubjectId})">
                    <i class="fas fa-eye"></i> View
                </button>
            </td>
            <td><span class="badge bg-info" id="avg-${student.studentSubjectId}">-</span></td>
            <td><span class="badge bg-success">Enrolled</span></td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="addGrade(${student.studentSubjectId})">
                    <i class="fas fa-plus"></i>
                </button>
            </td>
        `;
        tbody.appendChild(row);
        loadStudentGrades(student.studentSubjectId);
    });
}

async function loadStudentGrades(studentSubjectId) {
    try {
        const response = await fetch(`/api/v1/grade/studentsubject/${studentSubjectId}`);
        if (response.ok) {
            const grades = await response.json();
            updateGradesSummary(studentSubjectId, grades);
        }
    } catch (error) {
        console.error('Error loading grades:', error);
    }
}

function updateGradesSummary(studentSubjectId, grades) {
    const avgElement = document.getElementById(`avg-${studentSubjectId}`);
    if (grades.length > 0) {
        const avg = grades.reduce((sum, g) => sum + (g.percentage || 0), 0) / grades.length;
        avgElement.textContent = avg.toFixed(2) + '%';
        avgElement.className = avg >= 75 ? 'badge bg-success' : 'badge bg-warning';
    } else {
        avgElement.textContent = 'No grades';
        avgElement.className = 'badge bg-secondary';
    }
}

function addGrade(studentSubjectId) {
    document.getElementById('gradeStudentSubjectId').value = studentSubjectId;
    document.getElementById('gradeForm').reset();
    const modal = new bootstrap.Modal(document.getElementById('addGradeModal'));
    modal.show();
}

async function saveGrade() {
    const studentSubjectId = document.getElementById('gradeStudentSubjectId').value;
    const assessmentType = document.getElementById('assessmentType').value;
    const assessmentName = document.getElementById('assessmentName').value;
    const score = document.getElementById('score').value;
    const maxScore = document.getElementById('maxScore').value;
    const remarks = document.getElementById('remarks').value;

    const gradeData = {
        studentSubjectId: parseInt(studentSubjectId),
        assessmentType,
        assessmentName,
        score: parseFloat(score),
        maxScore: parseFloat(maxScore),
        remarks,
        dateGiven: new Date().toISOString()
    };

    try {
        const response = await fetch('/api/v1/grade', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(gradeData)
        });

        if (response.ok) {
            const modal = bootstrap.Modal.getInstance(document.getElementById('addGradeModal'));
            modal.hide();
            loadStudentGrades(studentSubjectId);
            showToast('Grade added successfully!', 'success');
        } else {
            const error = await response.json();
            showToast(error.message || 'Failed to add grade', 'error');
        }
    } catch (error) {
        console.error('Error adding grade:', error);
        showToast('Error adding grade', 'error');
    }
}

function showToast(message, type) {
    // Toast notification code (same as other pages)
}
```

### Add Grade Modal HTML (add before closing </body>):

```html
<div class="modal fade" id="addGradeModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Add Grade</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="gradeForm">
                    <input type="hidden" id="gradeStudentSubjectId">
                    <div class="mb-3">
                        <label for="assessmentType" class="form-label">Assessment Type *</label>
                        <select class="form-select" id="assessmentType" required>
                            <option value="Quiz">Quiz</option>
                            <option value="Exam">Exam</option>
                            <option value="Project">Project</option>
                            <option value="Assignment">Assignment</option>
                        </select>
                    </div>
                    <div class="mb-3">
                        <label for="assessmentName" class="form-label">Assessment Name *</label>
                        <input type="text" class="form-control" id="assessmentName" required>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="score" class="form-label">Score *</label>
                                <input type="number" class="form-control" id="score" step="0.01" required>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="maxScore" class="form-label">Max Score *</label>
                                <input type="number" class="form-control" id="maxScore" step="0.01" value="100" required>
                            </div>
                        </div>
                    </div>
                    <div class="mb-3">
                        <label for="remarks" class="form-label">Remarks</label>
                        <textarea class="form-control" id="remarks" rows="2"></textarea>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" onclick="saveGrade()">Save Grade</button>
            </div>
        </div>
    </div>
</div>
```

---

## Phase 5: Student Frontend Implementation

### Create Pages/Student/Enrollment.cshtml

This needs to be a NEW file with:
- Display student's current course/year/semester
- Show available subjects for enrollment
- Section selection per subject (with schedule, teacher, slots)
- Enroll button to submit

Key API calls:
- `GET /api/v1/enrollment/student/{studentId}` - Current enrollment
- `GET /api/v1/enrollment/available-subjects?courseId={}&yearLevelId={}&semesterId={}` - Available subjects
- `GET /api/v1/section/course/{courseId}/year/{yearLevelId}/semester/{semesterId}` - Sections
- `POST /api/v1/enrollment` - Create enrollment
- `POST /api/v1/studentsubject` - Enroll in subject

### Update Pages/Student/Grades.cshtml

Replace static HTML with:

```javascript
document.addEventListener('DOMContentLoaded', function() {
    loadStudentGrades();
});

async function loadStudentGrades() {
    try {
        // Get student ID from profile
        const profileResponse = await fetch('/api/v1/student/profile');
        if (!profileResponse.ok) return;
        
        const profile = await profileResponse.json();
        const studentId = profile.id;
        
        // Get grades
        const gradesResponse = await fetch(`/api/v1/grade/student/${studentId}`);
        if (gradesResponse.ok) {
            const grades = await response.json();
            displayGrades(grades);
        }
    } catch (error) {
        console.error('Error loading grades:', error);
    }
}

function displayGrades(grades) {
    const tbody = document.querySelector('tbody');
    tbody.innerHTML = '';
    
    // Group by subject
    const groupedBySubject = {};
    grades.forEach(grade => {
        const key = grade.subjectName;
        if (!groupedBySubject[key]) {
            groupedBySubject[key] = [];
        }
        groupedBySubject[key].push(grade);
    });
    
    // Display
    for (const [subject, subjectGrades] of Object.entries(groupedBySubject)) {
        subjectGrades.forEach(grade => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${subject}</td>
                <td>${grade.assessmentType} - ${grade.assessmentName}</td>
                <td>${grade.score}/${grade.maxScore}</td>
                <td>${grade.percentage}%</td>
                <td><span class="badge ${grade.percentage >= 75 ? 'bg-success' : 'bg-warning'}">${grade.percentage >= 75 ? 'Passed' : 'Review'}</span></td>
            `;
            tbody.appendChild(row);
        });
    }
}

// Add report generation buttons
async function generateReport() {
    try {
        const profileResponse = await fetch('/api/v1/student/profile');
        const profile = await profileResponse.json();
        
        window.open(`/api/v1/report/student/${profile.id}/grades/pdf`, '_blank');
    } catch (error) {
        console.error('Error generating report:', error);
    }
}

async function exportToExcel() {
    try {
        const profileResponse = await fetch('/api/v1/student/profile');
        const profile = await profileResponse.json();
        
        window.location.href = `/api/v1/report/student/${profile.id}/grades/excel`;
    } catch (error) {
        console.error('Error exporting to Excel:', error);
    }
}
```

Add buttons in header:
```html
<button class="btn btn-success me-2" onclick="generateReport()">
    <i class="fas fa-file-pdf me-1"></i>Generate Report
</button>
<button class="btn btn-info me-2" onclick="exportToExcel()">
    <i class="fas fa-file-excel me-1"></i>Export to Excel
</button>
```

---

## Phase 6: Report Generation

### Install NuGet Packages

```bash
dotnet add package QuestPDF --version 2024.3.0
dotnet add package EPPlus --version 7.0.0
```

### Create Services/ReportService.cs

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using StudentPeformanceTracker.Repository.Interfaces;

namespace StudentPeformanceTracker.Services;

public class ReportService
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentSubjectRepository _studentSubjectRepository;

    public ReportService(
        IGradeRepository gradeRepository,
        IStudentRepository studentRepository,
        IStudentSubjectRepository studentSubjectRepository)
    {
        _gradeRepository = gradeRepository;
        _studentRepository = studentRepository;
        _studentSubjectRepository = studentSubjectRepository;
    }

    public async Task<byte[]> GenerateStudentGradeReportPdf(int studentId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                page.Header().Text($"Grade Report - {student.FirstName} {student.LastName}")
                    .FontSize(20).Bold();
                
                page.Content().Column(column =>
                {
                    column.Item().Text($"Student ID: {student.StudentId}").FontSize(12);
                    column.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd}").FontSize(10);
                    
                    column.Item().PaddingVertical(10);
                    
                    // Grades table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });
                        
                        table.Header(header =>
                        {
                            header.Cell().Text("Subject");
                            header.Cell().Text("Assessment");
                            header.Cell().Text("Score");
                            header.Cell().Text("%");
                        });
                        
                        foreach (var grade in grades)
                        {
                            table.Cell().Text(grade.StudentSubject?.SectionSubject?.Subject?.SubjectName ?? "N/A");
                            table.Cell().Text($"{grade.AssessmentType} - {grade.AssessmentName}");
                            table.Cell().Text($"{grade.Score}/{grade.MaxScore}");
                            table.Cell().Text($"{grade.Percentage:F2}%");
                        }
                    });
                });
            });
        });
        
        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportStudentGradesToExcel(int studentId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);
        
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Grades");
        
        worksheet.Cells["A1"].Value = "Subject";
        worksheet.Cells["B1"].Value = "Assessment Type";
        worksheet.Cells["C1"].Value = "Assessment Name";
        worksheet.Cells["D1"].Value = "Score";
        worksheet.Cells["E1"].Value = "Max Score";
        worksheet.Cells["F1"].Value = "Percentage";
        worksheet.Cells["G1"].Value = "Remarks";
        
        int row = 2;
        foreach (var grade in grades)
        {
            worksheet.Cells[$"A{row}"].Value = grade.StudentSubject?.SectionSubject?.Subject?.SubjectName;
            worksheet.Cells[$"B{row}"].Value = grade.AssessmentType;
            worksheet.Cells[$"C{row}"].Value = grade.AssessmentName;
            worksheet.Cells[$"D{row}"].Value = grade.Score;
            worksheet.Cells[$"E{row}"].Value = grade.MaxScore;
            worksheet.Cells[$"F{row}"].Value = grade.Percentage;
            worksheet.Cells[$"G{row}"].Value = grade.Remarks;
            row++;
        }
        
        worksheet.Cells.AutoFitColumns();
        
        return package.GetAsByteArray();
    }
}
```

### Create Controllers/ReportController.cs

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPeformanceTracker.Services;

namespace StudentPeformanceTracker.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReportController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("student/{studentId}/grades/pdf")]
    [Authorize(Policy = "StudentOnly")]
    public async Task<IActionResult> GenerateStudentGradeReport(int studentId)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateStudentGradeReportPdf(studentId);
            return File(pdfBytes, "application/pdf", $"GradeReport_{studentId}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating report", error = ex.Message });
        }
    }

    [HttpGet("student/{studentId}/grades/excel")]
    [Authorize(Policy = "StudentOnly")]
    public async Task<IActionResult> ExportStudentGradesExcel(int studentId)
    {
        try
        {
            var excelBytes = await _reportService.ExportStudentGradesToExcel(studentId);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Grades_{studentId}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error exporting grades", error = ex.Message });
        }
    }
}
```

### Register in Program.cs

```csharp
builder.Services.AddScoped<ReportService>();
```

---

## Summary of Implementation Status

### ✅ Completed:
- Phase 1: Database Schema & Migration
- Phase 2: Backend API Implementation
- Phase 3: Admin Section Management
- Phase 4: Teacher My Classes page (fetches real data)

### 🔄 Requires Manual Updates:
- Phase 4: Teacher Grades.cshtml (add grade management modals & JavaScript)
- Phase 5: Student Enrollment page (CREATE NEW FILE)
- Phase 5: Student Grades.cshtml (update with real data + report buttons)
- Phase 6: Install packages, create ReportService & ReportController

### Next Steps:
1. Apply manual updates to Grades.cshtml
2. Create Student Enrollment page
3. Install QuestPDF and EPPlus packages
4. Create ReportService and ReportController
5. Test all functionality

