using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using eSaysay.Models;

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View("~/Views/User/Admin/Index.cshtml");
        }
        public IActionResult Analytics()
        {
            return View("~/Views/User/Admin/Analytics.cshtml");
        }
        public IActionResult Exercises()
        {
            return View("~/Views/User/Admin/Exercises.cshtml");
        }
        public IActionResult Lessons()
        {
            return View("~/Views/User/Admin/Lessons.cshtml");
        }
        public IActionResult Logs()
        {
            return View("~/Views/User/Admin/Logs.cshtml");
        }
        public IActionResult Progress()
        {
            return View("~/Views/User/Admin/Progress.cshtml");
        }
        public IActionResult Students()
        {
            return View("~/Views/User/Admin/Students.cshtml");
        }

        public IActionResult Settings()
        {
            return View("~/Views/User/Admin/Settings.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
