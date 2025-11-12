using System.Collections.Generic;

namespace GradingSystem.Models
{
    public class StudentGradeViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string YearLevel { get; set; } = string.Empty;
        public List<GradeDetail> Grades { get; set; } = new();
    }

    public class GradeDetail
    {
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public double? Prelim { get; set; }
        public double? Midterm { get; set; }
        public double? SemiFinal { get; set; }
        public double? Final { get; set; }
        public double? FinalGrade { get; set; }
    }
}
