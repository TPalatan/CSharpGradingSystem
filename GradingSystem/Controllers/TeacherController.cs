using Microsoft.AspNetCore.Mvc;

namespace GradingSystem.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Students()
        {
            return View();
        }

        public IActionResult Subjects()
        {
            return View();
        }

        public IActionResult Grades()
        {
            return View();
        }
    }
}
