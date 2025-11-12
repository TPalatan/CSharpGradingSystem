using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CSharpGradingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // 🔹 DASHBOARD
        // ==========================
        public IActionResult Dashboard()
        {
            var totalStudents = _context.Students.Count();
            var totalTeachers = _context.Teachers.Count();
            var totalSubjects = _context.Subjects.Count();
            var totalCourses = _context.Students.Select(s => s.Course).Distinct().Count();

            // Example: Recent activities (latest 5 student/teacher/subject additions)
            var recentActivities = _context.Students
                .OrderByDescending(s => s.Id)
                .Take(3)
                .Select(s => new RecentActivity
                {
                    Date = DateTime.Now,
                    Description = $"Student added: {s.FullName}",
                    PerformedBy = "Admin"
                }).ToList();

            recentActivities.AddRange(_context.Teachers
                .OrderByDescending(t => t.Id)
                .Take(3)
                .Select(t => new RecentActivity
                {
                    Date = DateTime.Now,
                    Description = $"Teacher added: {t.FullName}",
                    PerformedBy = "Admin"
                }));

            recentActivities.AddRange(_context.Subjects
                .OrderByDescending(su => su.Id)
                .Take(3)
                .Select(su => new RecentActivity
                {
                    Date = DateTime.Now,
                    Description = $"Subject added: {su.SubjectName}",
                    PerformedBy = "Admin"
                }));

            // Order by most recent
            recentActivities = recentActivities.OrderByDescending(a => a.Date).Take(5).ToList();

            var model = new DashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalSubjects = totalSubjects,
                TotalCourses = totalCourses,
                RecentActivities = recentActivities
            };

            return View(model);
        }

        // ==========================
        // 🔹 PENDING ACCOUNTS
        // ==========================
        [HttpGet]
        public IActionResult PendingAccounts()
        {
            var pendingUsers = _context.UserAccounts
                .Where(u => u.IsPending && !u.IsApproved)
                .ToList();

            if (!pendingUsers.Any())
            {
                TempData["InfoMessage"] = "No pending accounts at the moment.";
            }

            return View(pendingUsers);
        }

        // ✅ APPROVE ACCOUNT
        [HttpGet]
        public IActionResult Approve(int id)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(PendingAccounts));
            }

            user.IsApproved = true;
            user.IsPending = false;
            user.ApprovedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Account '{user.Email}' approved successfully!";
            return RedirectToAction(nameof(PendingAccounts));
        }

        // ❌ REJECT ACCOUNT
        [HttpGet]
        public IActionResult Reject(int id)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(PendingAccounts));
            }

            _context.UserAccounts.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Account '{user.Email}' has been rejected and removed.";
            return RedirectToAction(nameof(PendingAccounts));
        }

        // ==========================
        // 🔹 STUDENT MANAGEMENT
        // ==========================
        public IActionResult Student()
        {
            var students = _context.Students
                                   .Include(s => s.UserAccount)
                                   .ToList();
            return View(students);
        }




        // GET: Create Student
        [HttpGet]
        public IActionResult CreateStudent()
        {
            // Generate StudentID
            string yearPrefix = DateTime.Now.Year.ToString().Substring(2, 2);
            var latestStudent = _context.Students
                .Where(s => s.StudentID.StartsWith(yearPrefix))
                .OrderByDescending(s => s.StudentID)
                .FirstOrDefault();

            int nextNumber = 1;
            if (latestStudent != null)
            {
                string[] parts = latestStudent.StudentID.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                    nextNumber = lastNumber + 1;
            }

            string generatedId = $"{yearPrefix}-{nextNumber:D4}";
            var student = new Student { StudentID = generatedId };

            // Only approved "User" accounts not already assigned to a student
            var assignedUserIds = _context.Students.Select(s => s.UserAccountId).ToList();
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "User" && !assignedUserIds.Contains(u.Id))
                                           .OrderBy(u => u.Email)
                                           .ToList();

            return View(student);
        }

        // POST: Create Student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStudent(Student model)
        {
            // Check if the selected UserAccount is already assigned
            if (_context.Students.Any(s => s.UserAccountId == model.UserAccountId))
            {
                ModelState.AddModelError("UserAccountId", "This user account is already assigned to another student.");
            }

            if (ModelState.IsValid)
            {
                // Generate StudentID
                string yearPrefix = DateTime.Now.Year.ToString().Substring(2, 2);
                var latestStudent = _context.Students
                    .Where(s => s.StudentID.StartsWith(yearPrefix))
                    .OrderByDescending(s => s.StudentID)
                    .FirstOrDefault();

                int nextNumber = 1;
                if (latestStudent != null)
                {
                    string[] parts = latestStudent.StudentID.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                model.StudentID = $"{yearPrefix}-{nextNumber:D4}";

                _context.Students.Add(model);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Student added successfully! ID: {model.StudentID}";
                return RedirectToAction(nameof(Student));
            }

            // Reload approved accounts excluding already assigned ones
            var assignedUserIds = _context.Students.Select(s => s.UserAccountId).ToList();
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "User" && !assignedUserIds.Contains(u.Id))
                                           .OrderBy(u => u.Email)
                                           .ToList();

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(model);
        }





        // GET: Edit Student
        public IActionResult EditStudent(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null) return NotFound();

            PopulateDropdowns(student);
            return View(student);
        }

        // POST: Edit Student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStudent(Student student)
        {
            if (!ModelState.IsValid)
            {
                // Reload dropdowns if validation fails
                PopulateDropdowns(student);
                return View(student); // Return the same view with errors
            }

            try
            {
                _context.Update(student);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Student updated successfully!";
            }
            catch (Exception ex)
            {
                // Optional: log the error
                TempData["ErrorMessage"] = "Error updating student: " + ex.Message;
                PopulateDropdowns(student);
                return View(student);
            }

            return RedirectToAction("Student"); // Back to student list
        }

        // Helper method to populate dropdowns
        private void PopulateDropdowns(Student student)
        {
            ViewBag.Courses = new List<SelectListItem>
    {
        new SelectListItem { Text = "Bachelor of Science in Information Technology", Value = "Bachelor of Science in Information Technology" },
        new SelectListItem { Text = "Bachelor of Science in Business Administration", Value = "Bachelor of Science in Business Administration" },
        new SelectListItem { Text = "Bachelor of Secondary Education", Value = "Bachelor of Secondary Education" },
        new SelectListItem { Text = "Bachelor of Science in Criminology", Value = "Bachelor of Science in Criminology" }
    };

            ViewBag.YearLevels = new List<SelectListItem>
    {
        new SelectListItem { Text = "1st Year", Value = "1st Year" },
        new SelectListItem { Text = "2nd Year", Value = "2nd Year" },
        new SelectListItem { Text = "3rd Year", Value = "3rd Year" },
        new SelectListItem { Text = "4th Year", Value = "4th Year" }
    };

            ViewBag.StatusList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Active", Value = "Active" },
        new SelectListItem { Text = "Inactive", Value = "Inactive" }
    };

            ViewBag.UserAccounts = _context.UserAccounts
                .Select(u => new SelectListItem
                {
                    Text = u.Email,
                    Value = u.Id.ToString()
                }).ToList();
        }




        [HttpGet]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();

            _context.Students.Remove(student);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Student deleted successfully!";
            return RedirectToAction(nameof(Student));
        }

        // ==========================
        // 🔹 TEACHER MANAGEMENT
        // ==========================


        // GET: List all teachers

        // GET: List of teachers
        public IActionResult Teachers()
        {
            var teachers = _context.Teachers.ToList();
            return View(teachers);
        }


        [HttpGet]
        public IActionResult CreateTeacher()
        {
            // Emails already assigned to teachers
            var assignedEmails = _context.Teachers.Select(t => t.Email).ToList();

            // Only approved UserAccounts with role "Teacher" and not yet assigned
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "Teacher" && !assignedEmails.Contains(u.Email))
                                           .OrderBy(u => u.Email)
                                           .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTeacher(Teacher model)
        {
            if (ModelState.IsValid)
            {
                // Prevent duplicate email assignment
                if (_context.Teachers.Any(t => t.Email == model.Email))
                {
                    TempData["ErrorMessage"] = "This email is already assigned to another teacher.";
                }
                else
                {
                    // Map UserAccountId from selected email
                    var user = _context.UserAccounts
                                       .FirstOrDefault(u => u.Email == model.Email && u.IsApproved && u.Role == "Teacher");

                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Selected email does not exist or is not approved.";
                    }
                    else
                    {
                        model.UserAccountId = user.Id;
                        model.Email = user.Email;

                        _context.Teachers.Add(model);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "Teacher added successfully!";
                        return RedirectToAction(nameof(Teachers));
                    }
                }
            }

            // Reload dropdown if validation fails
            var assignedEmails = _context.Teachers.Select(t => t.Email).ToList();
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "Teacher" && !assignedEmails.Contains(u.Email))
                                           .OrderBy(u => u.Email)
                                           .ToList();

            return View(model);
        }



        // GET: Edit Teacher
        public IActionResult EditTeacher(int id)
        {
            var teacher = _context.Teachers.Find(id);
            if (teacher == null) return NotFound();

            PopulateDropdowns(teacher);
            return View(teacher);
        }

        // POST: Edit Teacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTeacher(Teacher teacher)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(teacher);
                return View(teacher); // Return the same view with validation errors
            }

            try
            {
                _context.Update(teacher);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Teacher updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating teacher: " + ex.Message;
                PopulateDropdowns(teacher);
                return View(teacher);
            }

            return RedirectToAction("Teachers"); // Back to teacher list
        }

        // Helper to populate dropdowns
        private void PopulateDropdowns(Teacher teacher)
        {
            ViewBag.Departments = new List<SelectListItem>
            {
                new SelectListItem { Text = "Department of Computer Studies", Value = "Department of Computer Studies" },
                new SelectListItem { Text = "Department of Business Administration", Value = "Department of Business Administration" },
                new SelectListItem { Text = "Department of Education", Value = "Department of Education" },
                new SelectListItem { Text = "Department of Criminal Justice", Value = "Department of Criminal Justice" }
            };

            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "Active" },
                new SelectListItem { Text = "Inactive", Value = "Inactive" }
            };

            ViewBag.UserAccounts = _context.UserAccounts
                .Select(u => new SelectListItem
                {
                    Text = u.Email,
                    Value = u.Id.ToString()
                }).ToList();
        }




        [HttpGet]
        public IActionResult DeleteTeacher(int id)
        {
            var teacher = _context.Teachers.FirstOrDefault(t => t.Id == id);
            if (teacher == null) return NotFound();

            _context.Teachers.Remove(teacher);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Teacher deleted successfully!";
            return RedirectToAction(nameof(Teachers));
        }

        // ==========================
        // 🔹 SUBJECT MANAGEMENT
        // ==========================
        public IActionResult Subjects()
        {
            var subjects = _context.Subjects
                                   .Include(s => s.AssignedTeacher)
                                   .ToList();

            return View(subjects);
        }

        [HttpGet]
        public IActionResult AddSubject()
        {
            ViewBag.Teachers = _context.Teachers.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSubject(Subject model)
        {
            if (ModelState.IsValid)
            {
                _context.Subjects.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Subject added successfully!";
                return RedirectToAction("Subjects");
            }

            ViewBag.Teachers = _context.Teachers.ToList();
            return View(model);
        }



        // GET: EditSubject
        public IActionResult EditSubject(int id)
        {
            var subject = _context.Subjects.Find(id);
            if (subject == null) return NotFound();

            // Dropdowns
            ViewBag.Departments = new List<SelectListItem>
    {
        new SelectListItem { Text = "Department of Computer Studies", Value = "Department of Computer Studies" },
        new SelectListItem { Text = "Department of Business Administration", Value = "Department of Business Administration" },
        new SelectListItem { Text = "Department of Education", Value = "Department of Education" },
        new SelectListItem { Text = "Department of Criminal Justice", Value = "Department of Criminal Justice" }
    };

            ViewBag.Teachers = _context.Teachers.Select(t => new SelectListItem
            {
                Text = t.FullName,
                Value = t.Id.ToString()
            }).ToList();

            ViewBag.Semesters = new List<SelectListItem>
    {
        new SelectListItem { Text = "1st Semester", Value = "1st Semester" },
        new SelectListItem { Text = "2nd Semester", Value = "2nd Semester" }
    };

            // Status options
            ViewBag.Statuses = new List<SelectListItem>
    {
        new SelectListItem { Text = "Active", Value = "Active" },
        new SelectListItem { Text = "Inactive", Value = "Inactive" }
    };

            return View(subject);
        }

        // POST: EditSubject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubject(Subject subject)
        {
            if (!ModelState.IsValid)
            {
                // Reload dropdowns if validation fails
                return RedirectToAction(nameof(EditSubject), new { id = subject.Id });
            }

            _context.Update(subject);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Subject updated successfully!";
            return RedirectToAction("Subjects");
        }




        [HttpGet]
        public IActionResult DeleteSubject(int id)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.Id == id);
            if (subject == null) return NotFound();

            _context.Subjects.Remove(subject);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Subject deleted successfully!";
            return RedirectToAction(nameof(Subjects));
        }

        // ==========================
        // 🔹 ASSIGN SUBJECT
        // ==========================
        public IActionResult AssignSubject()
        {
            ViewBag.Students = _context.Students.OrderBy(s => s.FullName).ToList();
            ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();

            var assignments = _context.StudentSubjectAssignments
                                      .Include(a => a.Student)
                                      .Include(a => a.Subject)
                                      .ToList();

            return View(assignments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignSubject(int studentId, int subjectId)
        {
            var exists = _context.StudentSubjectAssignments
                                 .Any(a => a.StudentId == studentId && a.SubjectId == subjectId);

            if (exists)
            {
                TempData["ErrorMessage"] = "This subject is already assigned to the selected student.";
            }
            else
            {
                var assignment = new StudentSubjectAssignment
                {
                    StudentId = studentId,
                    SubjectId = subjectId
                };
                _context.StudentSubjectAssignments.Add(assignment);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Subject assigned successfully!";
            }

            return RedirectToAction(nameof(AssignSubject));
        }

        public IActionResult DeleteAssignment(int id)
        {
            var assignment = _context.StudentSubjectAssignments.Find(id);
            if (assignment != null)
            {
                _context.StudentSubjectAssignments.Remove(assignment);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Assignment deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Assignment not found.";
            }

            return RedirectToAction(nameof(AssignSubject));
        }


        public async Task<IActionResult> StudentGrades()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                    .ThenInclude(s => s.AssignedTeacher)
                .ToListAsync();

            var studentGroups = grades
                .GroupBy(g => g.StudentId)
                .Select(g => new StudentGradeViewModel
                {
                    StudentId = g.Key,
                    StudentName = g.First().Student!.FullName,
                    Course = g.First().Student!.Course,
                    YearLevel = g.First().Student!.YearLevel,
                    Grades = g.Select(x => new GradeDetail
                    {
                        SubjectName = x.Subject!.SubjectName,
                        TeacherName = x.Subject.AssignedTeacher?.FullName ?? "",
                        Prelim = x.Prelim,
                        Midterm = x.Midterm,
                        SemiFinal = x.SemiFinal,
                        Final = x.Final,
                        FinalGrade = x.FinalGrade
                    }).ToList()
                })
                .OrderBy(s => s.Course)
                .ThenBy(s => s.YearLevel)
                .ToList();

            return View(studentGroups);
        }
    }
}
