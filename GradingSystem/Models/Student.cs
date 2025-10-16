using System.ComponentModel.DataAnnotations;

namespace GradingSystem.Models
{
    public class Student
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "StudentID")]
        public string StudentID { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Course { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Year Level")]
        public string YearLevel { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Active"; // Default value
    }
}
