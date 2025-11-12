using System;
using System.Collections.Generic;

namespace GradingSystem.Models
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalCourses { get; set; }  // Count of distinct courses

        public List<RecentActivity> RecentActivities { get; set; } = new();
    }

    public class RecentActivity
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
    }
}
