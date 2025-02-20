using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace eSaysay.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ListeningExercise()
        {
            return View("~/Views/exercises/listening_exercise.cshtml");
        }
    }
}
