using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradingSystem.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Subject Code is required")]
        [MaxLength(20)]
        [Display(Name = "Subject Code")]
        public string SubjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject Name is required")]
        [MaxLength(100)]
        [Display(Name = "Subject Name")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Units are required")]
        [Range(1, 10, ErrorMessage = "Units must be between 1 and 10")]
        public int Units { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [MaxLength(20)]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Assigned Teacher is required")]
        [Display(Name = "Assigned Teacher")]
        public int AssignedTeacherId { get; set; }

        // Navigation property for EF Core relationship
        [ForeignKey("AssignedTeacherId")]
        public Teacher? AssignedTeacher { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        // Not mapped property to populate dropdown in the view
        [NotMapped]
        public SelectList? TeacherList { get; set; }
    }
}
