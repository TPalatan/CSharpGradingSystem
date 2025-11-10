using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradingSystem.Models
{
    public class StudentSubjectAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }
    }

}
