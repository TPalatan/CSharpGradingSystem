using Microsoft.AspNetCore.Mvc;

namespace CSharpGradingSystem.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
