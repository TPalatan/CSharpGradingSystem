using CSharpGradingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSharpGradingSystem.Controllers
{
    public class UserController : Controller
    {

        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Grades()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }



        // GET: Student Profile
        public IActionResult StudentProfile()
        {
            // 🔹 Get logged-in user email from session
            var email = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(email))
            {
                // Not logged in, redirect to login page
                return RedirectToAction("Login", "Account");
            }

            // 🔹 Find the student associated with this email
            var student = _context.Students
                .Include(s => s.UserAccount)
                .FirstOrDefault(s => s.UserAccount != null && s.UserAccount.Email == email);

            if (student == null)
            {
                // No student found for this user
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Dashboard", "User");
            }

            return View(student);
        }



        public IActionResult Subjects()
        {
            return View();
        }
    }
}
