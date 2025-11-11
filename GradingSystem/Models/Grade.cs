using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradingSystem.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Range(0, 100, ErrorMessage = "Prelim grade must be between 0 and 100")]
        public double? Prelim { get; set; }

        [Range(0, 100, ErrorMessage = "Midterm grade must be between 0 and 100")]
        public double? Midterm { get; set; }

        [Range(0, 100, ErrorMessage = "SemiFinal grade must be between 0 and 100")]
        public double? SemiFinal { get; set; }

        [Range(0, 100, ErrorMessage = "Final grade must be between 0 and 100")]
        public double? Final { get; set; }

        public double? FinalGrade { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }
    }
}
