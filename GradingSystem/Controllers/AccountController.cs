using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CSharpGradingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 GET: Login Page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 🔹 POST: Handle Login
        [HttpPost]
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.UserAccounts
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // ✅ Save session data
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                // ✅ Redirect based on role
                if (user.Role == "Admin")
                {
                    TempData["SuccessMessage"] = "Welcome back, Admin!";
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Dashboard", "User");
                }
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }


        // 🔹 GET: Register
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 POST: Register new user
        [HttpPost]
        public IActionResult Create(UserAccount model)
        {
            if (ModelState.IsValid)
            {
                _context.UserAccounts.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Account created successfully! You can now log in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // 🔹 Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}
