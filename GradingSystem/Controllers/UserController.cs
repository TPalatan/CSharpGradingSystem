using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult Dashboard()
        {
            // Get logged-in user email
            var email = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            // Retrieve student info
            var student = _context.Students
                .Include(s => s.UserAccount)
                .FirstOrDefault(s => s.UserAccount != null && s.UserAccount.Email == email);

            return View(student);
        }

        public IActionResult Grades()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Subjects()
        {
            return View();
        }

        // GET: Student Profile
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

        // POST: Upload profile photo
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

            // Save the uploaded file
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profile");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{profilePic.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePic.CopyToAsync(fileStream);
            }

            // Save relative path to database
            student.ProfilePicturePath = "/uploads/profile/" + uniqueFileName;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Profile photo updated successfully.";
            return RedirectToAction("StudentProfile");
        }
    }
}
