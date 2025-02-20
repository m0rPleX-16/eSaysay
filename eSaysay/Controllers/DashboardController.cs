using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace eSaysay.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/User/Dashboard/Index.cshtml");
        }

        public IActionResult ListeningExercise()
        {
            return View("~/Views/User/Exercises/listening_exercise.cshtml");
        }
    }
}
