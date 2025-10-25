using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using StudentPeformanceTracker.Repository.Interfaces;
using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Services;

public class ReportService
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentSubjectRepository _studentSubjectRepository;
    private readonly ISectionSubjectRepository _sectionSubjectRepository;

    public ReportService(
        IGradeRepository gradeRepository,
        IStudentRepository studentRepository,
        IStudentSubjectRepository studentSubjectRepository,
        ISectionSubjectRepository sectionSubjectRepository)
    {
        _gradeRepository = gradeRepository;
        _studentRepository = studentRepository;
        _studentSubjectRepository = studentSubjectRepository;
        _sectionSubjectRepository = sectionSubjectRepository;
    }

    public async Task<byte[]> GenerateStudentGradeReportPdf(int studentId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            throw new ArgumentException("Student not found");
        }

        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);
        var gradesList = grades.ToList();

        // Group grades by subject
        var gradesBySubject = gradesList
            .GroupBy(g => new
            {
                SubjectName = g.StudentSubject?.SectionSubject?.Subject?.SubjectName ?? "Unknown Subject",
                SectionName = g.StudentSubject?.SectionSubject?.Section?.SectionName ?? "N/A"
            })
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header
                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("STUDENT GRADE REPORT")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    
                    column.Item().PaddingVertical(5);
                    column.Item().LineHorizontal(2).LineColor(Colors.Blue.Lighten2);
                    column.Item().PaddingVertical(5);
                    
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Student Name: {student.FirstName} {student.LastName}").Bold();
                        row.RelativeItem().AlignRight().Text($"Student ID: {student.StudentId}");
                    });
                    
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Course: {student.Course?.CourseName ?? "N/A"}");
                        row.RelativeItem().AlignRight().Text($"Year Level: {student.YearLevel?.ToString() ?? "N/A"}");
                    });
                    
                    column.Item().PaddingVertical(3);
                    column.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                // Content
                page.Content().PaddingVertical(10).Column(column =>
                {
                    if (gradesBySubject.Count == 0)
                    {
                        column.Item().Text("No grades recorded yet.").FontSize(14).Italic().FontColor(Colors.Grey.Medium);
                        return;
                    }

                    foreach (var subjectGroup in gradesBySubject)
                    {
                        column.Item().PaddingBottom(5).Text($"{subjectGroup.Key.SubjectName} ({subjectGroup.Key.SectionName})")
                            .FontSize(14).Bold().FontColor(Colors.Blue.Darken1);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Assessment
                                columns.RelativeColumn(1); // Score
                                columns.RelativeColumn(1); // Percentage
                                columns.RelativeColumn(2); // Remarks
                            });

                            // Table header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Assessment").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Score").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Percentage").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Remarks").Bold();
                            });

                            // Table rows
                            foreach (var grade in subjectGroup)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text($"{grade.AssessmentType} - {grade.AssessmentName}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text($"{grade.Score:F2}/{grade.MaxScore:F2}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text($"{grade.Percentage:F2}%");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(grade.Remarks ?? "-");
                            }

                            // Subject average
                            var subjectAvg = subjectGroup.Average(g => g.Percentage ?? 0);
                            table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten4).Padding(5).Text("Subject Average:").Bold();
                            table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten4).Padding(5)
                                .Text($"{subjectAvg:F2}%").Bold().FontColor(subjectAvg >= 75 ? Colors.Green.Darken1 : Colors.Orange.Darken1);
                        });

                        column.Item().PaddingBottom(15);
                    }

                    // Overall GPA
                    if (gradesList.Count > 0)
                    {
                        var overallAvg = gradesList.Average(g => g.Percentage ?? 0);
                        column.Item().PaddingTop(10).AlignRight().Row(row =>
                        {
                            row.AutoItem().Text("Overall Average: ").FontSize(14).Bold();
                            row.AutoItem().Text($"{overallAvg:F2}%").FontSize(14).Bold()
                                .FontColor(overallAvg >= 75 ? Colors.Green.Darken2 : Colors.Red.Darken1);
                        });
                    }
                });

                // Footer
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportStudentGradesToExcel(int studentId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            throw new ArgumentException("Student not found");
        }

        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);
        var gradesList = grades.ToList();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        
        // Student Info Sheet
        var infoSheet = package.Workbook.Worksheets.Add("Student Info");
        infoSheet.Cells["A1"].Value = "Student Information";
        infoSheet.Cells["A1:B1"].Merge = true;
        infoSheet.Cells["A1"].Style.Font.Bold = true;
        infoSheet.Cells["A1"].Style.Font.Size = 14;
        
        infoSheet.Cells["A2"].Value = "Name:";
        infoSheet.Cells["B2"].Value = $"{student.FirstName} {student.LastName}";
        infoSheet.Cells["A3"].Value = "Student ID:";
        infoSheet.Cells["B3"].Value = student.StudentId;
        infoSheet.Cells["A4"].Value = "Course:";
        infoSheet.Cells["B4"].Value = student.Course?.CourseName ?? "N/A";
        infoSheet.Cells["A5"].Value = "Year Level:";
        infoSheet.Cells["B5"].Value = student.YearLevel?.ToString() ?? "N/A";
        infoSheet.Cells["A6"].Value = "Generated:";
        infoSheet.Cells["B6"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        infoSheet.Cells["A2:A6"].Style.Font.Bold = true;
        infoSheet.Column(1).Width = 20;
        infoSheet.Column(2).Width = 30;

        // Grades Sheet
        var gradesSheet = package.Workbook.Worksheets.Add("Grades");
        
        // Headers
        gradesSheet.Cells["A1"].Value = "Subject";
        gradesSheet.Cells["B1"].Value = "Section";
        gradesSheet.Cells["C1"].Value = "Assessment Type";
        gradesSheet.Cells["D1"].Value = "Assessment Name";
        gradesSheet.Cells["E1"].Value = "Score";
        gradesSheet.Cells["F1"].Value = "Max Score";
        gradesSheet.Cells["G1"].Value = "Percentage";
        gradesSheet.Cells["H1"].Value = "Remarks";
        gradesSheet.Cells["I1"].Value = "Date Given";
        
        // Style headers
        using (var range = gradesSheet.Cells["A1:I1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // Data
        int row = 2;
        foreach (var grade in gradesList)
        {
            gradesSheet.Cells[$"A{row}"].Value = grade.StudentSubject?.SectionSubject?.Subject?.SubjectName ?? "N/A";
            gradesSheet.Cells[$"B{row}"].Value = grade.StudentSubject?.SectionSubject?.Section?.SectionName ?? "N/A";
            gradesSheet.Cells[$"C{row}"].Value = grade.AssessmentType;
            gradesSheet.Cells[$"D{row}"].Value = grade.AssessmentName;
            gradesSheet.Cells[$"E{row}"].Value = grade.Score;
            gradesSheet.Cells[$"F{row}"].Value = grade.MaxScore;
            gradesSheet.Cells[$"G{row}"].Value = grade.Percentage;
            gradesSheet.Cells[$"H{row}"].Value = grade.Remarks;
            gradesSheet.Cells[$"I{row}"].Value = grade.DateGiven?.ToString("yyyy-MM-dd") ?? "N/A";
            row++;
        }

        // Auto-fit columns
        gradesSheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task<byte[]> GenerateClassGradeReportPdf(int sectionSubjectId)
    {
        var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(sectionSubjectId);
        if (sectionSubject == null)
        {
            throw new ArgumentException("Section subject not found");
        }

        var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(sectionSubjectId);
        var studentSubjectsList = studentSubjects.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("CLASS GRADE REPORT")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    
                    column.Item().PaddingVertical(5);
                    column.Item().Text($"Subject: {sectionSubject.Subject?.SubjectName ?? "N/A"}").FontSize(14).Bold();
                    column.Item().Text($"Section: {sectionSubject.Section?.SectionName ?? "N/A"}").FontSize(12);
                    column.Item().Text($"Teacher: {sectionSubject.Teacher?.FirstName} {sectionSubject.Teacher?.LastName}").FontSize(12);
                    column.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Student Name
                        columns.RelativeColumn(1); // Student ID
                        columns.RelativeColumn(1); // Average
                        columns.RelativeColumn(1); // Status
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Student Name").Bold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Student ID").Bold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Average").Bold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Status").Bold();
                    });

                    foreach (var studentSubject in studentSubjectsList)
                    {
                        var student = studentSubject.Student;
                        var grades = studentSubject.Grades?.ToList() ?? new List<Grade>();
                        var average = grades.Any() ? grades.Average(g => g.Percentage ?? 0) : 0;

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"{student?.FirstName} {student?.LastName}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(student?.StudentId ?? "N/A");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"{average:F2}%");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(studentSubject.Status);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}

