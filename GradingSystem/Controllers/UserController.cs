using GradingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AuthDemo.Controllers
{
    [Authorize(Policy = "UserOnly")]
    public class UserController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Grades()
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



        public IActionResult AdminDashboard()
        {
            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

