using Microsoft.EntityFrameworkCore;
using GradingSystem.Models;

namespace CSharpGradingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; } = null!;
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Seed a default admin user
            modelBuilder.Entity<UserAccount>().HasData(
                new UserAccount
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123",   // ⚠️ For testing only — hash later
                    Role = "Admin",
                    IsApproved = true,
                    IsPending = false
                }
            );
        }
    }
}
