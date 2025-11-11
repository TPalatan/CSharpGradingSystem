using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CSharpGradingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserAccount> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<UserAccount>();
        }

        // 🔹 GET: Login Page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 🔹 POST: Handle Login with hashed password
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Wrong email or password.";
                return View();
            }

            // Verify hashed password
            var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Wrong email or password.";
                return View();
            }

            // Check approval status
            if (!user.IsApproved)
            {
                ViewBag.Error = user.IsPending
                    ? "Your account is still pending admin approval."
                    : "Your account was not approved by the admin.";
                return View();
            }

            // ✅ Save session data
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect based on role
            TempData["SuccessMessage"] = $"Welcome back, {user.Role}!";

            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                _ => RedirectToAction("Dashboard", "User") // Student
            };
        }

        // 🔹 GET: Register Page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 POST: Register new user with hashed password
        [HttpPost]
        public IActionResult Create(UserAccount model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            if (_context.UserAccounts.Any(u => u.Email == model.Email))
            {
                ViewBag.Error = "Email already exists. Please use another one.";
                return View(model);
            }

            // Hash password and set account status
            model.Password = _passwordHasher.HashPassword(model, model.Password);
            model.IsApproved = false;
            model.IsPending = true;

            _context.UserAccounts.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your account request has been sent. Please wait for admin approval.";
            return RedirectToAction("Login");
        }

        // 🔹 POST: Forgot Password
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Please enter your email.";
                return View();
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email not found.";
                return View();
            }

            // Generate random reset code
            var resetCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            // Store in TempData
            TempData["ResetCode"] = resetCode;
            TempData["Email"] = email;

            ViewBag.Message = "A reset code has been generated. (In production, this would be sent securely.)";
            ViewBag.ResetCode = resetCode;

            return View();
        }

        // 🔹 GET: Reset Password Page
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        // 🔹 POST: Reset Password Logic
        [HttpPost]
        public IActionResult ResetPassword(string email, string resetCode, string newPassword)
        {
            var storedCode = TempData["ResetCode"]?.ToString();
            var storedEmail = TempData["Email"]?.ToString();

            if (storedCode == null || storedEmail == null)
            {
                ViewBag.Error = "Session expired. Please request a new reset code.";
                return View();
            }

            if (resetCode != storedCode || email != storedEmail)
            {
                ViewBag.Error = "Invalid reset code or email.";
                return View();
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your password has been successfully reset. You can now log in.";
            return RedirectToAction("Login");
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
