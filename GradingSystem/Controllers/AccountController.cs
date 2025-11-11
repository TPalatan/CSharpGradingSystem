using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace CSharpGradingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserAccount> _passwordHasher;
        private readonly IConfiguration _config;

        public AccountController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<UserAccount>();
            _config = config;
        }

        // 🔹 GET: Login Page
        [HttpGet]
        public IActionResult Login() => View();

        // 🔹 POST: Login
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

            if (_passwordHasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Wrong email or password.";
                return View();
            }

            if (!user.IsApproved)
            {
                ViewBag.Error = user.IsPending
                    ? "Your account is still pending admin approval."
                    : "Your account was not approved by the admin.";
                return View();
            }

            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);
            TempData["SuccessMessage"] = $"Welcome back, {user.Role}!";

            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }

        // 🔹 GET: Register Page
        [HttpGet]
        public IActionResult Create() => View();

        // 🔹 POST: Register
        [HttpPost]
        public IActionResult Create(UserAccount model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.UserAccounts.Any(u => u.Email == model.Email))
            {
                ViewBag.Error = "Email already exists. Please use another one.";
                return View(model);
            }

            model.Password = _passwordHasher.HashPassword(model, model.Password);
            model.IsApproved = false;
            model.IsPending = true;

            _context.UserAccounts.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your account request has been sent. Please wait for admin approval.";
            return RedirectToAction("Login");
        }

        // 🔹 Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // 🔹 GET: Forgot Password
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // 🔹 POST: Forgot Password (send confirmation code)
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

            var code = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("ResetEmail", email);
            HttpContext.Session.SetString("ResetCode", code);

            try
            {
                SendEmail(email, "Password Reset Confirmation Code", $"Your confirmation code is: {code}");
                TempData["Success"] = "Confirmation code sent to your email.";
                return RedirectToAction("VerifyCode");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to send email: " + ex.Message;
                return View();
            }
        }

        // 🔹 GET: Verify Code
        [HttpGet]
        public IActionResult VerifyCode() => View();

        // 🔹 POST: Verify Code
        [HttpPost]
        public IActionResult VerifyCode(string code)
        {
            var sessionCode = HttpContext.Session.GetString("ResetCode");
            var email = HttpContext.Session.GetString("ResetEmail");

            if (string.IsNullOrEmpty(sessionCode) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Session expired. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            return code == sessionCode ? RedirectToAction("ResetPassword") : View((object)"Invalid code. Please try again.");
        }

        // 🔹 GET: Reset Password
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("ResetEmail")))
            {
                TempData["Error"] = "Session expired. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            return View();
        }

        // 🔹 POST: Reset Password
        [HttpPost]
        public IActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Session expired. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match or are empty.";
                return View();
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ForgotPassword");
            }

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            TempData["Success"] = "Password reset successfully. Please login.";
            HttpContext.Session.Remove("ResetEmail");
            HttpContext.Session.Remove("ResetCode");

            return RedirectToAction("Login");
        }

        // 🔹 Helper: Send Email
        private void SendEmail(string toEmail, string subject, string body)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"]);
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(smtpUser);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.EnableSsl = true;
                    client.Send(message);
                }
            }
        }
    }
}
