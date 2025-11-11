using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpGradingSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 🏠 Dashboard
        public IActionResult Dashboard()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var student = _context.Students
                .Include(s => s.UserAccount)
                .FirstOrDefault(s => s.UserAccount != null && s.UserAccount.Email == email);

            if (student == null)
                return RedirectToAction("StudentProfile");

            return View(student);
        }

        // 📚 Subjects
        public IActionResult Subjects()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var student = _context.Students
                .Include(s => s.UserAccount)
                .FirstOrDefault(s => s.UserAccount.Email == email);

            if (student == null)
                return RedirectToAction("Dashboard", "User");

            var subjects = _context.StudentSubjectAssignments
                .Where(ssa => ssa.StudentId == student.Id)
                .Include(ssa => ssa.Subject)
                .ThenInclude(s => s.AssignedTeacher)
                .Select(ssa => ssa.Subject)
                .ToList();

            ViewBag.StudentName = student.FullName;
            return View(subjects);
        }

        // 🧮 Grades Page
        public async Task<IActionResult> Grades()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var student = await _context.Students
                .Include(s => s.UserAccount)
                .FirstOrDefaultAsync(s => s.UserAccount != null && s.UserAccount.Email == email);

            if (student == null)
                return NotFound("Student not found.");

            var grades = await _context.Grades
                .Include(g => g.Subject)
                .Where(g => g.StudentId == student.Id)
                .ToListAsync();

            return View(grades);
        }

        // 👤 Student Profile
        public IActionResult StudentProfile()
        {
            var email = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var student = _context.Students
                .Include(s => s.UserAccount)
                .FirstOrDefault(s => s.UserAccount != null && s.UserAccount.Email == email);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Dashboard");
            }

            return View(student);
        }

        // 📸 Upload Profile Photo
        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile profilePic)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email) || profilePic == null)
                return RedirectToAction("StudentProfile");

            var student = _context.Students
                .Include(s => s.UserAccount)
                .FirstOrDefault(s => s.UserAccount != null && s.UserAccount.Email == email);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Dashboard");
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profile");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{profilePic.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePic.CopyToAsync(fileStream);
            }

            student.ProfilePicturePath = "/uploads/profile/" + uniqueFileName;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile photo updated successfully.";
            return RedirectToAction("StudentProfile");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
