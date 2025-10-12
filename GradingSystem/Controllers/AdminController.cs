using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemo.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();

        }
            public IActionResult Student()
        {
            return View();
        }

        public IActionResult Teachers()
        {
            return View();

        }


        public IActionResult Subjects()
        {
            return View();

        }
                  public IActionResult AssignSubjects()
        {
            return View();
        }
    }


}
