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
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Student configuration
            modelBuilder.Entity<Student>(entity =>
            {
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
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(t => t.User)
                    .WithOne(u => u.Teacher)
                    .HasForeignKey<Teacher>(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Admin configuration
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(a => a.User)
                    .WithOne(u => u.Admin)
                    .HasForeignKey<Admin>(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Course configuration
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => e.CourseName).IsUnique();
            });

            // Department configuration
            modelBuilder.Entity<Department>(entity =>
            {
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

        }
    }
}
