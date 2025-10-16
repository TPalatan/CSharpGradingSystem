using Microsoft.AspNetCore.Mvc;

namespace CSharpGradingSystem.Controllers
{
    public class UserController : Controller
    {
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

        public IActionResult StudentProfile()
        {
            return View();
        }

        public IActionResult Subjects()
        {
            return View();
        }
    }
}
