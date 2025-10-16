using System.ComponentModel.DataAnnotations;

namespace GradingSystem.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active or Inactive
    }
}
