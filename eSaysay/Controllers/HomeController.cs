using System.Diagnostics;
using eSaysay.Models;
using Microsoft.AspNetCore.Mvc;

namespace eSaysay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Views/User/Home/Index.cshtml");
        }

        public IActionResult About()
        {
            return View("~/Views/User/Home/About.cshtml");
        }

        public IActionResult Privacy()
        {
            return View("~/Views/User/Home/Privacy.cshtml");
        }

        public IActionResult Contact()
        {
            return View("~/Views/User/Home/Contact.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
