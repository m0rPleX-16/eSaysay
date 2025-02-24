using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using eSaysay.Models;
using System.Diagnostics;
using eSaysay.Models.Entities;
using eSaysay.Data;

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(ILogger<AdminController> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
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
            var exercises = _context.InteractiveExercises.Include(e => e.Lesson).ToList();
            return View("~/Views/User/Admin/Exercises.cshtml", exercises);
        }

        // Create Exercise
        [HttpPost]
        public async Task<IActionResult> CreateExercise(InteractiveExercise exercise)
        {
            if (ModelState.IsValid)
            {
                _context.InteractiveExercises.Add(exercise);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Exercises");
        }

        // Edit Exercise
        [HttpPost]
        public async Task<IActionResult> EditExercise(InteractiveExercise exercise)
        {
            if (ModelState.IsValid)
            {
                _context.InteractiveExercises.Update(exercise);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Exercises");
        }

        // Archive (Delete) Exercise
        [HttpPost]
        public async Task<IActionResult> ArchiveExercise(int ExerciseID)
        {
            var exercise = await _context.InteractiveExercises.FindAsync(ExerciseID);
            if (exercise != null)
            {
                _context.InteractiveExercises.Remove(exercise);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Exercises");
        }
        public IActionResult Lessons()
        {
            var lessons = _context.Lesson.Include(l => l.Language).ToList();
            return View("~/Views/User/Admin/Lessons.cshtml", lessons);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLesson(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                _context.Lesson.Add(lesson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Lessons");
        }

        [HttpPost]
        public async Task<IActionResult> EditLesson(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                _context.Lesson.Update(lesson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Lessons");
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveLesson(int LessonID)
        {
            var lesson = await _context.Lesson.FindAsync(LessonID);
            if (lesson != null)
            {
                _context.Lesson.Remove(lesson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Lessons");
        }

        public IActionResult Logs()
        {
            return View("~/Views/User/Admin/Logs.cshtml");
        }

        public IActionResult Progress()
        {
            return View("~/Views/User/Admin/Progress.cshtml");
        }

        public async Task<IActionResult> Students()
        {
            var users = await _userManager.Users.ToListAsync();

            var students = new List<IdentityUser>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    students.Add(user);
                }
            }

            return View("~/Views/User/Admin/Students.cshtml", students);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(string Id, string Email)
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Email))
            {
                return BadRequest("Invalid input.");
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Email = Email;
            user.UserName = Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Students");
            }

            return BadRequest("Failed to update user.");
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveStudent(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return BadRequest("Invalid input.");
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.MaxValue;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Students");
            }

            return BadRequest("Failed to archive user.");
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
