using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using StudentPeformanceTracker.Repository.Interfaces;
using StudentPeformanceTracker.Models;
using System.Drawing;

namespace StudentPeformanceTracker.Services;

public class ReportService
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentSubjectRepository _studentSubjectRepository;
    private readonly ISectionSubjectRepository _sectionSubjectRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public ReportService(
        IGradeRepository gradeRepository,
        IStudentRepository studentRepository,
        IStudentSubjectRepository studentSubjectRepository,
        ISectionSubjectRepository sectionSubjectRepository,
        ITeacherRepository teacherRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _gradeRepository = gradeRepository;
        _studentRepository = studentRepository;
        _studentSubjectRepository = studentSubjectRepository;
        _sectionSubjectRepository = sectionSubjectRepository;
        _teacherRepository = teacherRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<byte[]> GenerateStudentGradeReportPdf(int studentId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            throw new ArgumentException("Student not found");
        }

        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);
        // Filter to only Midterm and Final Grade
        var gradesList = grades
            .Where(g => g.AssessmentType == "Midterm" || g.AssessmentType == "Final Grade")
            .ToList();

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
                page.Margin(40);
                page.Size(PageSizes.Letter);

                // Header
                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Grade Report").FontSize(20).Bold();
                    column.Item().PaddingTop(10).AlignCenter().Text($"{student.FirstName} {student.LastName}")
                        .FontSize(16).SemiBold();
                    column.Item().AlignCenter().Text($"Student ID: {student.StudentId}")
                        .FontSize(12).FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(5).AlignCenter().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // Content
                page.Content().PaddingVertical(20).Column(column =>
                {
                    if (!gradesList.Any())
                    {
                        column.Item().Text("No grades available for this student.").FontSize(12).Italic();
                        return;
                    }

                    // Display grades by subject
                    foreach (var subjectGroup in gradesBySubject)
                    {
                        var midtermGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Midterm");
                        var finalGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Final Grade");

                        column.Item().PaddingBottom(10).Column(subCol =>
                        {
                            subCol.Item().Background(Colors.Blue.Lighten4).Padding(8).Text(text =>
                            {
                                text.Span($"{subjectGroup.Key.SubjectName}").FontSize(14).Bold();
                                text.Span($" ({subjectGroup.Key.SectionName})").FontSize(12).FontColor(Colors.Grey.Darken1);
                            });
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Grade Type").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Grade Point").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Remarks").Bold();
                            });

                            // Midterm row
                            if (midtermGrade != null)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text("Midterm");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text($"{midtermGrade.GradePoint:F2}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(midtermGrade.Remarks ?? "-");
                            }

                            // Final Grade row
                            if (finalGrade != null)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text("Final Grade");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text($"{finalGrade.GradePoint:F2}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(finalGrade.Remarks ?? "-");
                            }

                            if (midtermGrade == null && finalGrade == null)
                            {
                                table.Cell().ColumnSpan(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text("No grades recorded").FontColor(Colors.Grey.Medium);
                            }

                            // Subject average (Midterm 40% + Final 60%)
                            if (midtermGrade != null && finalGrade != null)
                            {
                                var subjectAvg = (midtermGrade.GradePoint * 0.4m) + (finalGrade.GradePoint * 0.6m);
                                table.Cell().ColumnSpan(1).Background(Colors.Grey.Lighten4).Padding(5).Text("Subject Average:").Bold();
                                table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten4).Padding(5)
                                    .Text($"{subjectAvg:F2}").Bold().FontColor(subjectAvg <= 3.0m ? Colors.Green.Darken1 : Colors.Red.Darken1);
                            }
                        });

                        column.Item().PaddingBottom(15);
                    }

                    // Overall GPA across all subjects
                    if (gradesList.Count > 0)
                    {
                        var allGradePoints = new List<decimal>();
                        foreach (var subjectGroup in gradesBySubject)
                        {
                            var midtermGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Midterm");
                            var finalGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Final Grade");
                            
                            if (midtermGrade != null && finalGrade != null)
                            {
                                var overallAvg = (midtermGrade.GradePoint * 0.4m) + (finalGrade.GradePoint * 0.6m);
                                allGradePoints.Add(overallAvg);
                            }
                            else if (midtermGrade != null)
                            {
                                allGradePoints.Add(midtermGrade.GradePoint);
                            }
                            else if (finalGrade != null)
                            {
                                allGradePoints.Add(finalGrade.GradePoint);
                            }
                        }
                        
                        if (allGradePoints.Count > 0)
                        {
                            var overallAvg = allGradePoints.Average();
                            column.Item().PaddingTop(10).AlignRight().Row(row =>
                            {
                                row.AutoItem().Text("Overall Grade Point Average (All Subjects): ").FontSize(14).Bold();
                                row.AutoItem().Text($"{overallAvg:F2}").FontSize(14).Bold()
                                    .FontColor(overallAvg <= 3.0m ? Colors.Green.Darken2 : Colors.Red.Darken1);
                            });
                        }
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
        // Filter to only Midterm and Final Grade
        var gradesList = grades
            .Where(g => g.AssessmentType == "Midterm" || g.AssessmentType == "Final Grade")
            .ToList();

        // Group grades by subject
        var gradesBySubject = gradesList
            .GroupBy(g => new
            {
                SubjectName = g.StudentSubject?.SectionSubject?.Subject?.SubjectName ?? "Unknown Subject",
                SectionName = g.StudentSubject?.SectionSubject?.Section?.SectionName ?? "N/A"
            })
            .ToList();

        // Set EPPlus license context for non-commercial use
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Grades");

        // Header
        worksheet.Cells["A1"].Value = "Student Grade Report";
        worksheet.Cells["A1:E1"].Merge = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        worksheet.Cells["A2"].Value = $"Name: {student.FirstName} {student.LastName}";
        worksheet.Cells["A3"].Value = $"Student ID: {student.StudentId}";
        worksheet.Cells["A4"].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy}";

        // Table headers
        int row = 6;
        worksheet.Cells[row, 1].Value = "Subject";
        worksheet.Cells[row, 2].Value = "Section";
        worksheet.Cells[row, 3].Value = "Midterm Grade";
        worksheet.Cells[row, 4].Value = "Final Grade";
        worksheet.Cells[row, 5].Value = "Overall Average";
        worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
        worksheet.Cells[row, 1, row, 5].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        // Data rows
        row++;
        foreach (var subjectGroup in gradesBySubject)
        {
            var midtermGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Midterm");
            var finalGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Final Grade");

            worksheet.Cells[row, 1].Value = subjectGroup.Key.SubjectName;
            worksheet.Cells[row, 2].Value = subjectGroup.Key.SectionName;
            worksheet.Cells[row, 3].Value = midtermGrade != null ? $"{midtermGrade.GradePoint:F2} ({midtermGrade.Remarks})" : "-";
            worksheet.Cells[row, 4].Value = finalGrade != null ? $"{finalGrade.GradePoint:F2} ({finalGrade.Remarks})" : "-";
            
            // Calculate overall average (40% midterm + 60% final)
            if (midtermGrade != null && finalGrade != null)
            {
                var overallAvg = (midtermGrade.GradePoint * 0.4m) + (finalGrade.GradePoint * 0.6m);
                worksheet.Cells[row, 5].Value = $"{overallAvg:F2}";
            }
            else
            {
                worksheet.Cells[row, 5].Value = "-";
            }
            row++;
        }

        // Calculate and add overall average
        var allGradePoints = new List<decimal>();
        foreach (var subjectGroup in gradesBySubject)
        {
            var midtermGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Midterm");
            var finalGrade = subjectGroup.FirstOrDefault(g => g.AssessmentType == "Final Grade");
            
            if (midtermGrade != null && finalGrade != null)
            {
                var overallAvg = (midtermGrade.GradePoint * 0.4m) + (finalGrade.GradePoint * 0.6m);
                allGradePoints.Add(overallAvg);
            }
            else if (midtermGrade != null)
            {
                allGradePoints.Add(midtermGrade.GradePoint);
            }
            else if (finalGrade != null)
            {
                allGradePoints.Add(finalGrade.GradePoint);
            }
        }
        
        if (allGradePoints.Count > 0)
        {
            row++;
            worksheet.Cells[row, 4].Value = "Overall GPA:";
            worksheet.Cells[row, 4].Style.Font.Bold = true;
            worksheet.Cells[row, 5].Value = $"{allGradePoints.Average():F2}";
            worksheet.Cells[row, 5].Style.Font.Bold = true;
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task<byte[]> GenerateClassGradeReportPdf(int sectionSubjectId)
    {
        var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(sectionSubjectId);
        if (sectionSubject == null)
        {
            throw new ArgumentException("Class not found");
        }

        var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(sectionSubjectId);
        var enrolledStudents = studentSubjects.Where(ss => ss.Status == "Enrolled").ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.Letter);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Class Grade Report").FontSize(20).Bold();
                    column.Item().PaddingTop(10).AlignCenter().Text($"{sectionSubject.Subject?.SubjectName ?? "N/A"}")
                        .FontSize(16).SemiBold();
                    column.Item().AlignCenter().Text($"Section: {sectionSubject.Section?.SectionName ?? "N/A"}")
                        .FontSize(12).FontColor(Colors.Grey.Medium);
                    column.Item().AlignCenter().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    if (!enrolledStudents.Any())
                    {
                        column.Item().Text("No students enrolled in this class.").FontSize(12).Italic();
                        return;
                    }

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("No.").Bold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Student Name").Bold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Midterm").Bold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Final").Bold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Average").Bold();
                        });

                        int index = 1;
                        foreach (var studentSubject in enrolledStudents)
                        {
                            // Pre-load grades before document creation to avoid async in lambda
                            var studentGrades = _gradeRepository.GetByStudentSubjectIdAsync(studentSubject.Id).Result;
                            var midtermGrade = studentGrades.FirstOrDefault(g => g.AssessmentType == "Midterm");
                            var finalGrade = studentGrades.FirstOrDefault(g => g.AssessmentType == "Final Grade");

                            var studentName = studentSubject.Student != null
                                ? $"{studentSubject.Student.FirstName} {studentSubject.Student.LastName}"
                                : "N/A";

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(index.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(studentName);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(midtermGrade != null ? $"{midtermGrade.GradePoint:F2}" : "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(finalGrade != null ? $"{finalGrade.GradePoint:F2}" : "-");
                            
                            if (midtermGrade != null && finalGrade != null)
                            {
                                var avg = (midtermGrade.GradePoint * 0.4m) + (finalGrade.GradePoint * 0.6m);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text($"{avg:F2}").FontColor(avg <= 3.0m ? Colors.Green.Darken1 : Colors.Red.Darken1);
                            }
                            else
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("-");
                            }

                            index++;
                        }
                    });
                });

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

    public async Task<byte[]> ExportClassGradesToExcel(int sectionSubjectId)
    {
        var sectionSubject = await _sectionSubjectRepository.GetByIdAsync(sectionSubjectId);
        if (sectionSubject == null)
        {
            throw new ArgumentException("Class not found");
        }

        var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(sectionSubjectId);
        var enrolledStudents = studentSubjects.Where(ss => ss.Status == "Enrolled").ToList();

        // Set EPPlus license context for non-commercial use
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Class Grades");

        // Header
        worksheet.Cells["A1"].Value = "Class Grade Report";
        worksheet.Cells["A1:F1"].Merge = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        worksheet.Cells["A2"].Value = $"Subject: {sectionSubject.Subject?.SubjectName ?? "N/A"}";
        worksheet.Cells["A3"].Value = $"Section: {sectionSubject.Section?.SectionName ?? "N/A"}";
        worksheet.Cells["A4"].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy}";

        // Table headers
        int row = 6;
        worksheet.Cells[row, 1].Value = "No.";
        worksheet.Cells[row, 2].Value = "Student ID";
        worksheet.Cells[row, 3].Value = "Student Name";
        worksheet.Cells[row, 4].Value = "Midterm Grade";
        worksheet.Cells[row, 5].Value = "Final Grade";
        worksheet.Cells[row, 6].Value = "Overall Average";
        worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
        worksheet.Cells[row, 1, row, 6].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        // Data rows
        row++;
        int index = 1;
        foreach (var studentSubject in enrolledStudents)
        {
            var studentGrades = await _gradeRepository.GetByStudentSubjectIdAsync(studentSubject.Id);
            var midtermGrade = studentGrades.FirstOrDefault(g => g.AssessmentType == "Midterm");
            var finalGrade = studentGrades.FirstOrDefault(g => g.AssessmentType == "Final Grade");

            var studentName = studentSubject.Student != null
                ? $"{studentSubject.Student.FirstName} {studentSubject.Student.LastName}"
                : "N/A";
            var studentId = studentSubject.Student?.StudentId ?? "N/A";

            worksheet.Cells[row, 1].Value = index;
            worksheet.Cells[row, 2].Value = studentId;
            worksheet.Cells[row, 3].Value = studentName;
            worksheet.Cells[row, 4].Value = midtermGrade != null ? $"{midtermGrade.GradePoint:F2} ({midtermGrade.Remarks})" : "-";
            worksheet.Cells[row, 5].Value = finalGrade != null ? $"{finalGrade.GradePoint:F2} ({finalGrade.Remarks})" : "-";
            
            // Calculate overall average (40% midterm + 60% final)
            if (midtermGrade != null && finalGrade != null)
            {
                var overallAvg = (midtermGrade.GradePoint * 0.4m) + (finalGrade.GradePoint * 0.6m);
                worksheet.Cells[row, 6].Value = $"{overallAvg:F2}";
                worksheet.Cells[row, 6].Style.Font.Color.SetColor(overallAvg <= 3.0m ? System.Drawing.Color.Green : System.Drawing.Color.Red);
            }
            else
            {
                worksheet.Cells[row, 6].Value = "-";
            }
            row++;
            index++;
        }

        // Calculate and add class statistics
        var allGradePoints = new List<decimal>();
        foreach (var studentSubject in enrolledStudents)
        {
            var studentGrades = await _gradeRepository.GetByStudentSubjectIdAsync(studentSubject.Id);
            var midtermGrade = studentGrades.FirstOrDefault(g => g.AssessmentType == "Midterm");
            var finalGrade = studentGrades.FirstOrDefault(g => g.AssessmentType == "Final Grade");
            
            if (midtermGrade != null && finalGrade != null)
            {
                var overallAvg = (midtermGrade.GradePoint * 0.4m) + (finalGrade.GradePoint * 0.6m);
                allGradePoints.Add(overallAvg);
            }
            else if (midtermGrade != null)
            {
                allGradePoints.Add(midtermGrade.GradePoint);
            }
            else if (finalGrade != null)
            {
                allGradePoints.Add(finalGrade.GradePoint);
            }
        }
        
        if (allGradePoints.Count > 0)
        {
            row++;
            worksheet.Cells[row, 5].Value = "Class Average:";
            worksheet.Cells[row, 5].Style.Font.Bold = true;
            worksheet.Cells[row, 6].Value = $"{allGradePoints.Average():F2}";
            worksheet.Cells[row, 6].Style.Font.Bold = true;
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task<byte[]> GenerateAnalyticsReportPdf()
    {
        // Get all data for analytics
        var students = (await _studentRepository.GetAllAsync()).ToList();
        var teachers = (await _teacherRepository.GetAllAsync()).ToList();
        var courses = (await _courseRepository.GetAllAsync()).ToList();
        var enrollments = (await _enrollmentRepository.GetAllAsync()).ToList();
        var grades = (await _gradeRepository.GetAllAsync()).ToList();
        var sectionSubjects = (await _sectionSubjectRepository.GetAllAsync()).ToList();

        // Calculate statistics
        var totalStudents = students.Count;
        var activeTeachers = teachers.Count;
        var totalCourses = courses.Count;
        
        // Calculate average performance
        var gradePoints = grades
            .Where(g => g.GradePoint > 0 && (g.AssessmentType == "Midterm" || g.AssessmentType == "Final Grade"))
            .Select(g => g.GradePoint)
            .ToList();
        var avgPerformance = gradePoints.Any() 
            ? ((5.0m - gradePoints.Average()) / 4.0m) * 100m 
            : 0m;

        // Enrollment by course
        var enrollmentByCourse = enrollments
            .GroupBy(e => e.Course?.CourseName ?? "Unknown")
            .Select(g => new { Course = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        // Grade distribution
        var gradeDistribution = new Dictionary<string, int>
        {
            { "A (90-100%)", 0 },
            { "B (80-89%)", 0 },
            { "C (70-79%)", 0 },
            { "D (60-69%)", 0 },
            { "F (Below 60%)", 0 }
        };

        foreach (var grade in gradePoints)
        {
            var percentage = ((5.0m - grade) / 4.0m) * 100m;
            if (percentage >= 90) gradeDistribution["A (90-100%)"]++;
            else if (percentage >= 80) gradeDistribution["B (80-89%)"]++;
            else if (percentage >= 70) gradeDistribution["C (70-79%)"]++;
            else if (percentage >= 60) gradeDistribution["D (60-69%)"]++;
            else gradeDistribution["F (Below 60%)"]++;
        }

        // Pass/Fail rate
        var passed = gradePoints.Count(g => ((5.0m - g) / 4.0m) * 100m >= 60);
        var failed = gradePoints.Count - passed;
        var passRate = gradePoints.Any() ? (passed * 100m / gradePoints.Count) : 0m;
        var failRate = gradePoints.Any() ? (failed * 100m / gradePoints.Count) : 0m;

        // Faculty load
        var facultyLoad = new List<(string Name, int Classes, int Students)>();
        foreach (var teacher in teachers.Take(10))
        {
            var teacherClasses = sectionSubjects.Where(ss => ss.TeacherId == teacher.Id).ToList();
            var studentCount = 0;
            foreach (var ss in teacherClasses)
            {
                var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(ss.Id);
                studentCount += studentSubjects.Count(s => s.Status == "Enrolled" || s.Status == "Pending");
            }
            facultyLoad.Add(($"{teacher.FirstName} {teacher.LastName}", teacherClasses.Count, studentCount));
        }
        facultyLoad = facultyLoad.OrderByDescending(f => f.Students).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.Letter);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Analytics Dashboard Report").FontSize(24).Bold();
                    column.Item().PaddingTop(5).AlignCenter().Text($"Generated: {DateTime.Now:MMMM dd, yyyy 'at' HH:mm}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    // Key Metrics
                    column.Item().PaddingBottom(10).Text("Key Metrics").FontSize(18).Bold();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().PaddingRight(5).Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Total Students").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{totalStudents}").FontSize(20).Bold();
                        });
                        row.RelativeItem().PaddingHorizontal(5).Background(Colors.Green.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Active Faculty").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{activeTeachers}").FontSize(20).Bold();
                        });
                        row.RelativeItem().PaddingHorizontal(5).Background(Colors.Orange.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Courses Offered").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{totalCourses}").FontSize(20).Bold();
                        });
                        row.RelativeItem().PaddingLeft(5).Background(Colors.Purple.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Avg. Performance").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{avgPerformance:F1}%").FontSize(20).Bold();
                        });
                    });

                    column.Item().PaddingTop(20);

                    // Enrollment by Course
                    column.Item().PaddingBottom(10).Text("Student Enrollment by Course").FontSize(16).Bold();
                    if (enrollmentByCourse.Any())
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Course").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Students").Bold();
                            });

                            foreach (var item in enrollmentByCourse)
                            {
                                table.Cell().Element(CellStyle).Text(item.Course);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Count.ToString());
                            }
                        });
                    }
                    else
                    {
                        column.Item().Text("No enrollment data available").FontColor(Colors.Grey.Medium);
                    }

                    column.Item().PaddingTop(15);

                    // Grade Distribution
                    column.Item().PaddingBottom(10).Text("Grade Distribution").FontSize(16).Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Grade Range").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Count").Bold();
                        });

                        foreach (var item in gradeDistribution)
                        {
                            table.Cell().Element(CellStyle).Text(item.Key);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.Value.ToString());
                        }
                    });

                    column.Item().PaddingTop(15);

                    // Pass/Fail Rate
                    column.Item().PaddingBottom(10).Text("Pass/Fail Rate").FontSize(16).Bold();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().PaddingRight(5).Background(Colors.Green.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Pass Rate").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{passRate:F1}%").FontSize(18).Bold();
                            col.Item().Text($"({passed} students)").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                        row.RelativeItem().PaddingLeft(5).Background(Colors.Red.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Fail Rate").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{failRate:F1}%").FontSize(18).Bold();
                            col.Item().Text($"({failed} students)").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    column.Item().PaddingTop(15);

                    // Faculty Load
                    column.Item().PaddingBottom(10).Text("Faculty Load Distribution (Top 10)").FontSize(16).Bold();
                    if (facultyLoad.Any())
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Faculty Name").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Classes").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Students").Bold();
                            });

                            foreach (var faculty in facultyLoad)
                            {
                                table.Cell().Element(CellStyle).Text(faculty.Name);
                                table.Cell().Element(CellStyle).AlignRight().Text(faculty.Classes.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(faculty.Students.ToString());
                            }
                        });
                    }
                    else
                    {
                        column.Item().Text("No faculty load data available").FontColor(Colors.Grey.Medium);
                    }
                });

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

    public async Task<byte[]> ExportAnalyticsToExcel()
    {
        // Get all data for analytics
        var students = (await _studentRepository.GetAllAsync()).ToList();
        var teachers = (await _teacherRepository.GetAllAsync()).ToList();
        var courses = (await _courseRepository.GetAllAsync()).ToList();
        var enrollments = (await _enrollmentRepository.GetAllAsync()).ToList();
        var grades = (await _gradeRepository.GetAllAsync()).ToList();
        var sectionSubjects = (await _sectionSubjectRepository.GetAllAsync()).ToList();

        // Calculate statistics
        var totalStudents = students.Count;
        var activeTeachers = teachers.Count;
        var totalCourses = courses.Count;
        
        // Calculate average performance
        var gradePoints = grades
            .Where(g => g.GradePoint > 0 && (g.AssessmentType == "Midterm" || g.AssessmentType == "Final Grade"))
            .Select(g => g.GradePoint)
            .ToList();
        var avgPerformance = gradePoints.Any() 
            ? ((5.0m - gradePoints.Average()) / 4.0m) * 100m 
            : 0m;

        // Enrollment by course
        var enrollmentByCourse = enrollments
            .GroupBy(e => e.Course?.CourseName ?? "Unknown")
            .Select(g => new { Course = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        // Grade distribution
        var gradeDistribution = new Dictionary<string, int>
        {
            { "A (90-100%)", 0 },
            { "B (80-89%)", 0 },
            { "C (70-79%)", 0 },
            { "D (60-69%)", 0 },
            { "F (Below 60%)", 0 }
        };

        foreach (var grade in gradePoints)
        {
            var percentage = ((5.0m - grade) / 4.0m) * 100m;
            if (percentage >= 90) gradeDistribution["A (90-100%)"]++;
            else if (percentage >= 80) gradeDistribution["B (80-89%)"]++;
            else if (percentage >= 70) gradeDistribution["C (70-79%)"]++;
            else if (percentage >= 60) gradeDistribution["D (60-69%)"]++;
            else gradeDistribution["F (Below 60%)"]++;
        }

        // Pass/Fail rate
        var passed = gradePoints.Count(g => ((5.0m - g) / 4.0m) * 100m >= 60);
        var failed = gradePoints.Count - passed;
        var passRate = gradePoints.Any() ? (passed * 100m / gradePoints.Count) : 0m;
        var failRate = gradePoints.Any() ? (failed * 100m / gradePoints.Count) : 0m;

        // Faculty load
        var facultyLoad = new List<(string Name, int Classes, int Students)>();
        foreach (var teacher in teachers.Take(10))
        {
            var teacherClasses = sectionSubjects.Where(ss => ss.TeacherId == teacher.Id).ToList();
            var studentCount = 0;
            foreach (var ss in teacherClasses)
            {
                var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(ss.Id);
                studentCount += studentSubjects.Count(s => s.Status == "Enrolled" || s.Status == "Pending");
            }
            facultyLoad.Add(($"{teacher.FirstName} {teacher.LastName}", teacherClasses.Count, studentCount));
        }
        facultyLoad = facultyLoad.OrderByDescending(f => f.Students).ToList();

        // Set EPPlus license context for non-commercial use
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        
        // Key Metrics Sheet
        var metricsSheet = package.Workbook.Worksheets.Add("Key Metrics");
        metricsSheet.Cells["A1"].Value = "Analytics Dashboard Report";
        metricsSheet.Cells["A1:D1"].Merge = true;
        metricsSheet.Cells["A1"].Style.Font.Size = 16;
        metricsSheet.Cells["A1"].Style.Font.Bold = true;
        metricsSheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        metricsSheet.Cells["A2"].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy 'at' HH:mm}";
        metricsSheet.Cells["A2:D2"].Merge = true;
        metricsSheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        int row = 4;
        metricsSheet.Cells[row, 1].Value = "Metric";
        metricsSheet.Cells[row, 2].Value = "Value";
        metricsSheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
        metricsSheet.Cells[row, 1, row, 2].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        row++;
        metricsSheet.Cells[row, 1].Value = "Total Students";
        metricsSheet.Cells[row, 2].Value = totalStudents;
        row++;
        metricsSheet.Cells[row, 1].Value = "Active Faculty";
        metricsSheet.Cells[row, 2].Value = activeTeachers;
        row++;
        metricsSheet.Cells[row, 1].Value = "Courses Offered";
        metricsSheet.Cells[row, 2].Value = totalCourses;
        row++;
        metricsSheet.Cells[row, 1].Value = "Average Performance";
        metricsSheet.Cells[row, 2].Value = $"{avgPerformance:F1}%";
        row++;
        metricsSheet.Cells[row, 1].Value = "Pass Rate";
        metricsSheet.Cells[row, 2].Value = $"{passRate:F1}% ({passed} students)";
        row++;
        metricsSheet.Cells[row, 1].Value = "Fail Rate";
        metricsSheet.Cells[row, 2].Value = $"{failRate:F1}% ({failed} students)";

        metricsSheet.Cells.AutoFitColumns();

        // Enrollment by Course Sheet
        var enrollmentSheet = package.Workbook.Worksheets.Add("Enrollment by Course");
        enrollmentSheet.Cells["A1"].Value = "Student Enrollment by Course";
        enrollmentSheet.Cells["A1:B1"].Merge = true;
        enrollmentSheet.Cells["A1"].Style.Font.Size = 14;
        enrollmentSheet.Cells["A1"].Style.Font.Bold = true;
        enrollmentSheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        row = 3;
        enrollmentSheet.Cells[row, 1].Value = "Course";
        enrollmentSheet.Cells[row, 2].Value = "Students";
        enrollmentSheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
        enrollmentSheet.Cells[row, 1, row, 2].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        row++;
        foreach (var item in enrollmentByCourse)
        {
            enrollmentSheet.Cells[row, 1].Value = item.Course;
            enrollmentSheet.Cells[row, 2].Value = item.Count;
            row++;
        }

        enrollmentSheet.Cells.AutoFitColumns();

        // Grade Distribution Sheet
        var gradeDistSheet = package.Workbook.Worksheets.Add("Grade Distribution");
        gradeDistSheet.Cells["A1"].Value = "Grade Distribution";
        gradeDistSheet.Cells["A1:B1"].Merge = true;
        gradeDistSheet.Cells["A1"].Style.Font.Size = 14;
        gradeDistSheet.Cells["A1"].Style.Font.Bold = true;
        gradeDistSheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        row = 3;
        gradeDistSheet.Cells[row, 1].Value = "Grade Range";
        gradeDistSheet.Cells[row, 2].Value = "Count";
        gradeDistSheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
        gradeDistSheet.Cells[row, 1, row, 2].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        row++;
        foreach (var item in gradeDistribution)
        {
            gradeDistSheet.Cells[row, 1].Value = item.Key;
            gradeDistSheet.Cells[row, 2].Value = item.Value;
            row++;
        }

        gradeDistSheet.Cells.AutoFitColumns();

        // Faculty Load Sheet
        var facultySheet = package.Workbook.Worksheets.Add("Faculty Load");
        facultySheet.Cells["A1"].Value = "Faculty Load Distribution (Top 10)";
        facultySheet.Cells["A1:C1"].Merge = true;
        facultySheet.Cells["A1"].Style.Font.Size = 14;
        facultySheet.Cells["A1"].Style.Font.Bold = true;
        facultySheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        row = 3;
        facultySheet.Cells[row, 1].Value = "Faculty Name";
        facultySheet.Cells[row, 2].Value = "Classes";
        facultySheet.Cells[row, 3].Value = "Students";
        facultySheet.Cells[row, 1, row, 3].Style.Font.Bold = true;
        facultySheet.Cells[row, 1, row, 3].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        row++;
        foreach (var faculty in facultyLoad)
        {
            facultySheet.Cells[row, 1].Value = faculty.Name;
            facultySheet.Cells[row, 2].Value = faculty.Classes;
            facultySheet.Cells[row, 3].Value = faculty.Students;
            row++;
        }

        facultySheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task<byte[]> GenerateTeacherAnalyticsReportPdf(int teacherId)
    {
        var teacher = await _teacherRepository.GetByIdAsync(teacherId);
        if (teacher == null)
        {
            throw new ArgumentException("Teacher not found");
        }

        // Get teacher's classes
        var sectionSubjects = (await _sectionSubjectRepository.GetByTeacherIdAsync(teacherId)).ToList();
        var sectionSubjectIds = sectionSubjects.Select(ss => ss.Id).ToList();

        // Get all students and grades for teacher's classes
        var allStudents = new List<StudentSubject>();
        var allGrades = new List<Grade>();

        foreach (var sectionSubjectId in sectionSubjectIds)
        {
            var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(sectionSubjectId);
            allStudents.AddRange(studentSubjects.Where(s => s.Status == "Enrolled" || s.Status == "Pending"));

            foreach (var studentSubject in studentSubjects)
            {
                var grades = await _gradeRepository.GetByStudentSubjectIdAsync(studentSubject.Id);
                allGrades.AddRange(grades);
            }
        }

        // Calculate statistics
        var totalClasses = sectionSubjects.Count;
        var totalStudents = allStudents.Count;
        var gradePoints = allGrades
            .Where(g => g.GradePoint > 0 && (g.AssessmentType == "Midterm" || g.AssessmentType == "Final Grade"))
            .Select(g => g.GradePoint)
            .ToList();
        var averageGrade = gradePoints.Any() ? gradePoints.Average() : 0m;
        var passCount = gradePoints.Count(g => g <= 3.0m);
        var failCount = gradePoints.Count - passCount;
        var passRate = gradePoints.Any() ? (passCount * 100m / gradePoints.Count) : 0m;

        // Grade distribution
        var gradeDistribution = new Dictionary<string, int>
        {
            { "Excellent (1.0-1.5)", 0 },
            { "Very Good (1.6-2.0)", 0 },
            { "Good (2.1-2.5)", 0 },
            { "Pass (2.6-3.0)", 0 },
            { "Failed (3.1-5.0)", 0 }
        };

        foreach (var grade in gradePoints)
        {
            if (grade >= 1.0m && grade <= 1.5m) gradeDistribution["Excellent (1.0-1.5)"]++;
            else if (grade > 1.5m && grade <= 2.0m) gradeDistribution["Very Good (1.6-2.0)"]++;
            else if (grade > 2.0m && grade <= 2.5m) gradeDistribution["Good (2.1-2.5)"]++;
            else if (grade > 2.5m && grade <= 3.0m) gradeDistribution["Pass (2.6-3.0)"]++;
            else if (grade > 3.0m && grade <= 5.0m) gradeDistribution["Failed (3.1-5.0)"]++;
        }

        // Class performance
        var classPerformance = new List<(string ClassName, decimal Average)>();
        foreach (var sectionSubject in sectionSubjects)
        {
            var classGrades = allGrades.Where(g => 
                allStudents.Any(s => s.SectionSubjectId == sectionSubject.Id && s.Id == g.StudentSubjectId)
            ).Select(g => g.GradePoint).Where(g => g > 0).ToList();
            
            if (classGrades.Any())
            {
                classPerformance.Add((
                    $"{sectionSubject.Subject?.SubjectName ?? "N/A"} ({sectionSubject.Section?.SectionName ?? "N/A"})",
                    classGrades.Average()
                ));
            }
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.Letter);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Teacher Analytics Report").FontSize(24).Bold();
                    column.Item().PaddingTop(5).AlignCenter().Text($"{teacher.FirstName} {teacher.LastName}")
                        .FontSize(16).SemiBold();
                    column.Item().PaddingTop(5).AlignCenter().Text($"Generated: {DateTime.Now:MMMM dd, yyyy 'at' HH:mm}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    // Key Metrics
                    column.Item().PaddingBottom(10).Text("Key Metrics").FontSize(18).Bold();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().PaddingRight(5).Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Total Classes").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{totalClasses}").FontSize(20).Bold();
                        });
                        row.RelativeItem().PaddingHorizontal(5).Background(Colors.Green.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Total Students").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{totalStudents}").FontSize(20).Bold();
                        });
                        row.RelativeItem().PaddingHorizontal(5).Background(Colors.Orange.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Average Grade").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{averageGrade:F2}").FontSize(20).Bold();
                        });
                        row.RelativeItem().PaddingLeft(5).Background(Colors.Purple.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Pass Rate").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{passRate:F1}%").FontSize(20).Bold();
                        });
                    });

                    column.Item().PaddingTop(20);

                    // Grade Distribution
                    column.Item().PaddingBottom(10).Text("Grade Distribution").FontSize(16).Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Grade Range").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Count").Bold();
                        });

                        foreach (var item in gradeDistribution)
                        {
                            table.Cell().Element(CellStyle).Text(item.Key);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.Value.ToString());
                        }
                    });

                    column.Item().PaddingTop(15);

                    // Pass/Fail Rate
                    column.Item().PaddingBottom(10).Text("Pass/Fail Rate").FontSize(16).Bold();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().PaddingRight(5).Background(Colors.Green.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Pass Rate").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{passRate:F1}%").FontSize(18).Bold();
                            col.Item().Text($"({passCount} students)").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                        row.RelativeItem().PaddingLeft(5).Background(Colors.Red.Lighten4).Padding(15).Column(col =>
                        {
                            col.Item().Text("Fail Rate").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{(100m - passRate):F1}%").FontSize(18).Bold();
                            col.Item().Text($"({failCount} students)").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    column.Item().PaddingTop(15);

                    // Class Performance
                    if (classPerformance.Any())
                    {
                        column.Item().PaddingBottom(10).Text("Average Grade by Class").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Class").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Average Grade").Bold();
                            });

                            foreach (var item in classPerformance.OrderByDescending(c => c.Average))
                            {
                                table.Cell().Element(CellStyle).Text(item.ClassName);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Average.ToString("F2"));
                            }
                        });
                    }
                });

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

    public async Task<byte[]> ExportTeacherAnalyticsToExcel(int teacherId)
    {
        var teacher = await _teacherRepository.GetByIdAsync(teacherId);
        if (teacher == null)
        {
            throw new ArgumentException("Teacher not found");
        }

        // Get teacher's classes
        var sectionSubjects = (await _sectionSubjectRepository.GetByTeacherIdAsync(teacherId)).ToList();
        var sectionSubjectIds = sectionSubjects.Select(ss => ss.Id).ToList();

        // Get all students and grades for teacher's classes
        var allStudents = new List<StudentSubject>();
        var allGrades = new List<Grade>();

        foreach (var sectionSubjectId in sectionSubjectIds)
        {
            var studentSubjects = await _studentSubjectRepository.GetBySectionSubjectIdAsync(sectionSubjectId);
            allStudents.AddRange(studentSubjects.Where(s => s.Status == "Enrolled" || s.Status == "Pending"));

            foreach (var studentSubject in studentSubjects)
            {
                var grades = await _gradeRepository.GetByStudentSubjectIdAsync(studentSubject.Id);
                allGrades.AddRange(grades);
            }
        }

        // Calculate statistics
        var totalClasses = sectionSubjects.Count;
        var totalStudents = allStudents.Count;
        var gradePoints = allGrades
            .Where(g => g.GradePoint > 0 && (g.AssessmentType == "Midterm" || g.AssessmentType == "Final Grade"))
            .Select(g => g.GradePoint)
            .ToList();
        var averageGrade = gradePoints.Any() ? gradePoints.Average() : 0m;
        var passCount = gradePoints.Count(g => g <= 3.0m);
        var failCount = gradePoints.Count - passCount;
        var passRate = gradePoints.Any() ? (passCount * 100m / gradePoints.Count) : 0m;

        // Grade distribution
        var gradeDistribution = new Dictionary<string, int>
        {
            { "Excellent (1.0-1.5)", 0 },
            { "Very Good (1.6-2.0)", 0 },
            { "Good (2.1-2.5)", 0 },
            { "Pass (2.6-3.0)", 0 },
            { "Failed (3.1-5.0)", 0 }
        };

        foreach (var grade in gradePoints)
        {
            if (grade >= 1.0m && grade <= 1.5m) gradeDistribution["Excellent (1.0-1.5)"]++;
            else if (grade > 1.5m && grade <= 2.0m) gradeDistribution["Very Good (1.6-2.0)"]++;
            else if (grade > 2.0m && grade <= 2.5m) gradeDistribution["Good (2.1-2.5)"]++;
            else if (grade > 2.5m && grade <= 3.0m) gradeDistribution["Pass (2.6-3.0)"]++;
            else if (grade > 3.0m && grade <= 5.0m) gradeDistribution["Failed (3.1-5.0)"]++;
        }

        // Class performance
        var classPerformance = new List<(string ClassName, decimal Average)>();
        foreach (var sectionSubject in sectionSubjects)
        {
            var classGrades = allGrades.Where(g => 
                allStudents.Any(s => s.SectionSubjectId == sectionSubject.Id && s.Id == g.StudentSubjectId)
            ).Select(g => g.GradePoint).Where(g => g > 0).ToList();
            
            if (classGrades.Any())
            {
                classPerformance.Add((
                    $"{sectionSubject.Subject?.SubjectName ?? "N/A"} ({sectionSubject.Section?.SectionName ?? "N/A"})",
                    classGrades.Average()
                ));
            }
        }

        // Set EPPlus license context for non-commercial use
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        
        // Key Metrics Sheet
        var metricsSheet = package.Workbook.Worksheets.Add("Key Metrics");
        metricsSheet.Cells["A1"].Value = "Teacher Analytics Report";
        metricsSheet.Cells["A1:D1"].Merge = true;
        metricsSheet.Cells["A1"].Style.Font.Size = 16;
        metricsSheet.Cells["A1"].Style.Font.Bold = true;
        metricsSheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        metricsSheet.Cells["A2"].Value = $"Teacher: {teacher.FirstName} {teacher.LastName}";
        metricsSheet.Cells["A2:D2"].Merge = true;
        metricsSheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        metricsSheet.Cells["A3"].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy 'at' HH:mm}";
        metricsSheet.Cells["A3:D3"].Merge = true;
        metricsSheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        int row = 5;
        metricsSheet.Cells[row, 1].Value = "Metric";
        metricsSheet.Cells[row, 2].Value = "Value";
        metricsSheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
        metricsSheet.Cells[row, 1, row, 2].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        row++;
        metricsSheet.Cells[row, 1].Value = "Total Classes";
        metricsSheet.Cells[row, 2].Value = totalClasses;
        row++;
        metricsSheet.Cells[row, 1].Value = "Total Students";
        metricsSheet.Cells[row, 2].Value = totalStudents;
        row++;
        metricsSheet.Cells[row, 1].Value = "Average Grade";
        metricsSheet.Cells[row, 2].Value = averageGrade.ToString("F2");
        row++;
        metricsSheet.Cells[row, 1].Value = "Pass Rate";
        metricsSheet.Cells[row, 2].Value = $"{passRate:F1}% ({passCount} students)";
        row++;
        metricsSheet.Cells[row, 1].Value = "Fail Rate";
        metricsSheet.Cells[row, 2].Value = $"{(100m - passRate):F1}% ({failCount} students)";

        metricsSheet.Cells.AutoFitColumns();

        // Grade Distribution Sheet
        var gradeDistSheet = package.Workbook.Worksheets.Add("Grade Distribution");
        gradeDistSheet.Cells["A1"].Value = "Grade Distribution";
        gradeDistSheet.Cells["A1:B1"].Merge = true;
        gradeDistSheet.Cells["A1"].Style.Font.Size = 14;
        gradeDistSheet.Cells["A1"].Style.Font.Bold = true;
        gradeDistSheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        row = 3;
        gradeDistSheet.Cells[row, 1].Value = "Grade Range";
        gradeDistSheet.Cells[row, 2].Value = "Count";
        gradeDistSheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
        gradeDistSheet.Cells[row, 1, row, 2].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

        row++;
        foreach (var item in gradeDistribution)
        {
            gradeDistSheet.Cells[row, 1].Value = item.Key;
            gradeDistSheet.Cells[row, 2].Value = item.Value;
            row++;
        }

        gradeDistSheet.Cells.AutoFitColumns();

        // Class Performance Sheet
        if (classPerformance.Any())
        {
            var classSheet = package.Workbook.Worksheets.Add("Class Performance");
            classSheet.Cells["A1"].Value = "Average Grade by Class";
            classSheet.Cells["A1:B1"].Merge = true;
            classSheet.Cells["A1"].Style.Font.Size = 14;
            classSheet.Cells["A1"].Style.Font.Bold = true;
            classSheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            row = 3;
            classSheet.Cells[row, 1].Value = "Class";
            classSheet.Cells[row, 2].Value = "Average Grade";
            classSheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
            classSheet.Cells[row, 1, row, 2].Style.Fill.SetBackground(System.Drawing.Color.LightBlue);

            row++;
            foreach (var item in classPerformance.OrderByDescending(c => c.Average))
            {
                classSheet.Cells[row, 1].Value = item.ClassName;
                classSheet.Cells[row, 2].Value = item.Average.ToString("F2");
                row++;
            }

            classSheet.Cells.AutoFitColumns();
        }

        return package.GetAsByteArray();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(5)
            .PaddingHorizontal(5);
    }
}
