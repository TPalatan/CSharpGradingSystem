using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradingSystem.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string StudentID { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Course { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string YearLevel { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string Status { get; set; } = "Active";

        // Foreign key to UserAccount
        [Display(Name = "User Account")]
        public int? UserAccountId { get; set; }  // nullable
        public UserAccount? UserAccount { get; set; }

    }
}
