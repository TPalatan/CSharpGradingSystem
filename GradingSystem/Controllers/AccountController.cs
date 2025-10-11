using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthDemo.Models;

namespace AuthDemo.Controllers
{
    public class AccountController : Controller
    {
        // Hardcoded users for demo (replace with a database later)
        private readonly List<User> _users = new()
        {
            new User { Username = "admin", Password = "admin123", Role = "Admin" },
            new User { Username = "user", Password = "user123", Role = "User" }
        };

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // Create user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role == "Admin")
                return RedirectToAction("Dashboard", "Admin");
            else
                return RedirectToAction("Dashboard", "User");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => Content("Access Denied");
    }
}
