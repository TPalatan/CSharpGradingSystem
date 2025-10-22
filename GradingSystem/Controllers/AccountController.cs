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
        public IActionResult Login(string username, string password)
        {
            var user = _context.UserAccounts
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // 🚫 Check if the account is approved
                if (!user.IsApproved)
                {
                    ViewBag.Error = user.IsPending
                        ? "Your account is still pending admin approval."
                        : "Your account was not approved by the admin.";
                    return View();
                }

                // ✅ Save session data
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                // ✅ Redirect based on role
                if (user.Role == "Admin")
                {
                    TempData["SuccessMessage"] = "Welcome back, Admin!";
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (user.Role == "Teacher")
                {
                    TempData["SuccessMessage"] = "Welcome back, Teacher!";
                    return RedirectToAction("Dashboard", "Teacher");
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

        // 🔹 GET: Register Page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 POST: Register new user (requires admin approval)
        [HttpPost]
        public IActionResult Create(UserAccount model)
        {
            if (ModelState.IsValid)
            {
                // 🟡 Mark new account as pending approval
                model.IsApproved = false;
                model.IsPending = true;

                _context.UserAccounts.Add(model);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your account request has been sent. Please wait for admin approval.";
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
