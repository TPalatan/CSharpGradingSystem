using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GradingSystem.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(15)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active or Inactive

        // Foreign key to UserAccount
        [Display(Name = "User Account")]
        public int? UserAccountId { get; set; }  // nullable
        public UserAccount? UserAccount { get; set; }

        // New property for profile picture
        public string? ProfilePicturePath { get; set; }

        // 🔹 Navigation property for subjects
        public ICollection<Subject>? Subjects { get; set; }
    }
}
