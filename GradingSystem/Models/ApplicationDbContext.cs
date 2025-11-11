using Microsoft.EntityFrameworkCore;
using GradingSystem.Models;
using System;

namespace CSharpGradingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<StudentSubjectAssignment> StudentSubjectAssignments { get; set; } = null!;

        public DbSet<Grade> Grades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Only use HasData for static data (no dynamic values)
            // Example:
            // modelBuilder.Entity<Subject>().HasData(
            //     new Subject { Id = 1, Name = "English", Code = "ENG101" }
            // );
        }

        // 🔹 Removed SeedAdmin method
        // You will manually insert the admin in the database
    }
}
