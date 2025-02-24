using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Student")]
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
        public IActionResult exitListeningExercise()
        {
            return View("~/Views/User/Dashboard/Index.cshtml");
        }

        public IActionResult CorrectTranslation()
        {
            return View("~/Views/User/Exercises/corr_translation.cshtml");
        }
        public IActionResult exitCorrectExercise()
        {
            return View("~/Views/User/Dashboard/Index.cshtml");
        }

        public IActionResult CompleteTranslation()
        {
            return View("~/Views/User/Exercises/comp_translation.cshtml");
        }
        public IActionResult exitCompleteExercise()
        {
            return View("~/Views/User/Dashboard/Index.cshtml");
        }

        public IActionResult PairingExercise()
        {
            return View("~/Views/User/Exercises/pairing.cshtml");
        }
        public IActionResult exitPairingExercise()
        {
            return View("~/Views/User/Dashboard/Index.cshtml");
        }

    }
}
