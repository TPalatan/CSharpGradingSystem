using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Login(string username, string password)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ViewBag.Error = "Wrong username or password.";
                return View();
            }

            // ✅ Verify hashed password
            var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Wrong username or password.";
                return View();
            }

            // 🚫 Check approval status
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
            if (ModelState.IsValid)
            {
                // ✅ Hash the password before saving
                model.Password = _passwordHasher.HashPassword(model, model.Password);

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





        // 🔹 POST: Generate and show reset code
        [HttpPost]
        public IActionResult ForgotPassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Please enter your username.";
                return View();
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "Username not found.";
                return View();
            }

            // Generate random reset code (for demo)
            var resetCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            // Store in TempData for simplicity (in real app, store in DB)
            TempData["ResetCode"] = resetCode;
            TempData["Username"] = username;

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
        public IActionResult ResetPassword(string username, string resetCode, string newPassword)
        {
            var storedCode = TempData["ResetCode"]?.ToString();
            var storedUser = TempData["Username"]?.ToString();

            if (storedCode == null || storedUser == null)
            {
                ViewBag.Error = "Session expired. Please request a new reset code.";
                return View();
            }

            if (resetCode != storedCode || username != storedUser)
            {
                ViewBag.Error = "Invalid reset code or username.";
                return View();
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            // ✅ Update hashed password
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
