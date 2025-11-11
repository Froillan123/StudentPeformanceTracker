using Microsoft.EntityFrameworkCore;
using StudentPeformanceTracker.Models;

namespace StudentPeformanceTracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Authentication and Authorization
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<TeacherDepartment> TeacherDepartments { get; set; }

        // New entities for course and subject management
        public DbSet<YearLevel> YearLevels { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<CourseSubject> CourseSubjects { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionSubject> SectionSubjects { get; set; }
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<StudentSubject> StudentSubjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Announcement> Announcements { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DateTime properties to be stored as UTC
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp with time zone");
                    }
                }
            }

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Student configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.StudentId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(s => s.User)
                    .WithOne(u => u.Student)
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(s => s.Course)
                    .WithMany()
                    .HasForeignKey(s => s.CourseId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Teacher configuration
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(t => t.User)
                    .WithOne(u => u.Teacher)
                    .HasForeignKey<Teacher>(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Admin configuration
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(a => a.User)
                    .WithOne(u => u.Admin)
                    .HasForeignKey<Admin>(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Course configuration
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.CourseName).IsUnique();
            });

            // Department configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.DepartmentName).IsUnique();
                entity.HasIndex(e => e.DepartmentCode).IsUnique();
            });

            // Course configuration
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasOne(c => c.Department)
                    .WithMany()
                    .HasForeignKey(c => c.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TeacherDepartment many-to-many junction table configuration
            modelBuilder.Entity<TeacherDepartment>(entity =>
            {
                // Composite primary key
                entity.HasKey(td => new { td.TeacherId, td.DepartmentId });

                // Teacher relationship
                entity.HasOne(td => td.Teacher)
                    .WithMany(t => t.TeacherDepartments)
                    .HasForeignKey(td => td.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Department relationship
                entity.HasOne(td => td.Department)
                    .WithMany(d => d.TeacherDepartments)
                    .HasForeignKey(td => td.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // YearLevel configuration
            modelBuilder.Entity<YearLevel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.LevelNumber).IsUnique();
            });

            // Semester configuration
            modelBuilder.Entity<Semester>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.SemesterCode).IsUnique();
                entity.HasIndex(e => new { e.SemesterCode, e.SchoolYear }).IsUnique();
            });

            // Subject configuration
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                // No unique indexes needed for Subject
            });

            // CourseSubject configuration
            modelBuilder.Entity<CourseSubject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.CourseId, e.SubjectId, e.YearLevelId, e.SemesterId }).IsUnique();
            });

            // Section configuration
            modelBuilder.Entity<Section>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.SectionName, e.CourseId, e.YearLevelId, e.SemesterId }).IsUnique();
            });

            // SectionSubject configuration
            modelBuilder.Entity<SectionSubject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.EdpCode).IsUnique();
                entity.HasIndex(e => new { e.SectionId, e.SubjectId }).IsUnique();
            });

            // TeacherSubject configuration
            modelBuilder.Entity<TeacherSubject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.TeacherId, e.SectionSubjectId }).IsUnique();
            });

            // Enrollment configuration
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.StudentId, e.CourseId, e.YearLevelId, e.SemesterId }).IsUnique();
            });

            // StudentSubject configuration
            modelBuilder.Entity<StudentSubject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.StudentId, e.SectionSubjectId }).IsUnique();
            });

            // Grade configuration
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            // Announcement configuration
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasOne(a => a.Teacher)
                    .WithMany()
                    .HasForeignKey(a => a.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
                
                entity.HasOne(a => a.SectionSubject)
                    .WithMany()
                    .HasForeignKey(a => a.SectionSubjectId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
                
                entity.HasOne(a => a.Admin)
                    .WithMany()
                    .HasForeignKey(a => a.AdminId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
            });

        }
    }
}
