using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Student()
        {
            var students = _context.Students.ToList();
            return View(students);
        }

        [HttpGet]
        public IActionResult CreateStudent()
        {
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

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStudent(Student model)
        {
            if (ModelState.IsValid)
            {
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

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(model);
        }

        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null)
                return NotFound();

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

            TempData["ErrorMessage"] = "Update failed. Please try again.";
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null)
                return NotFound();

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
        public IActionResult Subjects()
        {
            return View();
        }

        public IActionResult AssignSubject()
        {
            return View();
        }
    }
}
