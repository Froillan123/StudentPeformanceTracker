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



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Teacher-Department relationship
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasOne(t => t.Department)
                    .WithMany(d => d.Teachers)
                    .HasForeignKey(t => t.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

        }
    }
}
