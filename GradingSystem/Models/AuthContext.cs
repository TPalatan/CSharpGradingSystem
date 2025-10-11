// Models/AuthContext.cs
using System.Data.Entity;

namespace AuthDemo.Models
{
    public class AuthContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
