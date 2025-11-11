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
            var pendingUsers = _context.UserAccounts
                .Where(u => u.IsPending)
                .ToList();

            ViewBag.PendingCount = pendingUsers.Count;
            return View(pendingUsers);
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

            // ✅ Only approved "User" accounts
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "User")
                                           .OrderBy(u => u.Email) // sorted by Email
                                           .ToList();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStudent(Student model)
        {
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

            // Reload approved accounts
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved)
                                           .OrderBy(u => u.Email)
                                           .ToList();

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(model);
        }





        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();

            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved)
                                           .OrderBy(u => u.Email)
                                           .ToList();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStudent(Student model)
        {
            if (ModelState.IsValid)
            {
                _context.Students.Update(model);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Student updated successfully!";
                return RedirectToAction(nameof(Student));
            }

            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved)
                                           .OrderBy(u => u.Email)
                                           .ToList();

            TempData["ErrorMessage"] = "Update failed. Please try again.";
            return View(model);
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
            // Only approved user accounts for email selection
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "Teacher")
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
                // Map selected Email to UserAccountId
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var user = _context.UserAccounts
                                       .FirstOrDefault(u => u.Email == model.Email && u.Role == "Teacher");
                    if (user != null)
                    {
                        model.UserAccountId = user.Id;
                        model.Email = user.Email; // optional, just to ensure consistency
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Selected email does not exist.";
                        ViewBag.UserAccounts = _context.UserAccounts
                                                       .Where(u => u.IsApproved && u.Role == "Teacher")
                                                       .OrderBy(u => u.Email)
                                                       .ToList();
                        return View(model);
                    }
                }

                _context.Teachers.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Teacher added successfully!";
                return RedirectToAction(nameof(Teachers));
            }

            // Reload dropdown if validation fails
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "Teacher")
                                           .OrderBy(u => u.Email)
                                           .ToList();

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(model);
        }



        [HttpGet]
        public IActionResult EditTeacher(int id)
        {
            var teacher = _context.Teachers.FirstOrDefault(t => t.Id == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTeacher(Teacher model)
        {
            if (ModelState.IsValid)
            {
                _context.Teachers.Update(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Teacher updated successfully!";
                return RedirectToAction(nameof(Teachers));
            }

            TempData["ErrorMessage"] = "Update failed. Please try again.";
            return View(model);
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

        [HttpGet]
        public IActionResult EditSubject(int id)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.Id == id);
            if (subject == null) return NotFound();

            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubject(Subject model)
        {
            if (ModelState.IsValid)
            {
                _context.Subjects.Update(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Subject updated successfully!";
                return RedirectToAction(nameof(Subjects));
            }

            TempData["ErrorMessage"] = "Update failed. Please try again.";
            return View(model);
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
    }
}
