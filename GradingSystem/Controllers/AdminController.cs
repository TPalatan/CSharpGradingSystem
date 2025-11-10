using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Mvc;
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
            // Show all pending users count
            var pendingUsers = _context.UserAccounts
                .Where(u => u.IsPending)
                .ToList();

            ViewBag.PendingCount = pendingUsers.Count;
            return View(pendingUsers);
        }

        // ==========================
        // 🔹 PENDING ACCOUNTS PAGE
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
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Account '{user.Username}' approved successfully!";
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

            TempData["SuccessMessage"] = $"Account '{user.Username}' has been rejected and removed.";
            return RedirectToAction(nameof(PendingAccounts));
        }

        // ==========================
        // 🔹 STUDENT MANAGEMENT
        // ==========================
        // ==========================
        // 🔹 LIST STUDENTS
        // ==========================
        public IActionResult Student()
        {
            var students = _context.Students
                                   .Include(s => s.UserAccount)
                                   .ToList();
            return View(students);
        }

        // ==========================
        // 🔹 CREATE STUDENT (GET)
        // ==========================
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

            // ✅ Only get users with Role = "User" and approved
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.Role == "User")
                                           .OrderBy(u => u.Username)
                                           .ToList();

            return View(student);
        }


        // ==========================
        // 🔹 CREATE STUDENT (POST)
        // ==========================
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

            // Reload approved user accounts if validation fails
            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.IsPending == false)
                                           .OrderBy(u => u.Username)
                                           .ToList();

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(model);
        }

        // ==========================
        // 🔹 EDIT STUDENT (GET)
        // ==========================
        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();

            ViewBag.UserAccounts = _context.UserAccounts
                                           .Where(u => u.IsApproved && u.IsPending == false)
                                           .OrderBy(u => u.Username)
                                           .ToList();

            return View(student);
        }

        // ==========================
        // 🔹 EDIT STUDENT (POST)
        // ==========================
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
                                           .Where(u => u.IsApproved && u.IsPending == false)
                                           .OrderBy(u => u.Username)
                                           .ToList();

            TempData["ErrorMessage"] = "Update failed. Please try again.";
            return View(model);
        }

        // ==========================
        // 🔹 DELETE STUDENT
        // ==========================
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
        public IActionResult Teachers()
        {
            var teachers = _context.Teachers.ToList();
            return View(teachers);
        }

        [HttpGet]
        public IActionResult CreateTeacher()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTeacher(Teacher model)
        {
            if (ModelState.IsValid)
            {
                _context.Teachers.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Teacher added successfully!";
                return RedirectToAction(nameof(Teachers));
            }

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(model);
        }

        [HttpGet]
        public IActionResult EditTeacher(int id)
        {
            var teacher = _context.Teachers.FirstOrDefault(t => t.Id == id);
            if (teacher == null)
                return NotFound();

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
            if (teacher == null)
                return NotFound();

            _context.Teachers.Remove(teacher);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Teacher deleted successfully!";
            return RedirectToAction(nameof(Teachers));
        }

        // ==========================
        // 🔹 SUBJECT MANAGEMENT
        // ==========================

        // ==========================
        // 🔹 LIST ALL SUBJECTS
        // ==========================


        // GET: /Admin/Subjects
        public IActionResult Subjects()
        {
            // Eager load AssignedTeacher so we can display the name in the table
            var subjects = _context.Subjects
                                   .Include(s => s.AssignedTeacher) // load teacher
                                   .ToList();

            return View(subjects);
        }

        // GET: /Admin/AddSubject
        public IActionResult AddSubject()
        {
            // Load teachers to display in dropdown
            ViewBag.Teachers = _context.Teachers.ToList();
            return View();
        }

        // POST: /Admin/AddSubject
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

            // Reload teachers if validation fails
            ViewBag.Teachers = _context.Teachers.ToList();
            return View(model);
        }


        // ==========================
        // 🔹 EDIT SUBJECT (GET)
        // ==========================
        [HttpGet]
        public IActionResult EditSubject(int id)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.Id == id);
            if (subject == null)
                return NotFound();

            return View(subject);
        }

        // ==========================
        // 🔹 EDIT SUBJECT (POST)
        // ==========================
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

        // ==========================
        // 🔹 DELETE SUBJECT
        // ==========================
        [HttpGet]
        public IActionResult DeleteSubject(int id)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.Id == id);
            if (subject == null)
                return NotFound();

            _context.Subjects.Remove(subject);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Subject deleted successfully!";
            return RedirectToAction(nameof(Subjects));
        }



        //Assign Subject to Student
        // ==========================
        // 🔹 ASSIGN SUBJECT (GET)

        // GET: Assign Subject
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

        // POST: Assign Subject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignSubject(int studentId, int subjectId)
        {
            // Check if the assignment already exists
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

        // Delete Assignment
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
     