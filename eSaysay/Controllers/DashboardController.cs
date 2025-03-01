using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using eSaysay.Data;
using Microsoft.AspNetCore.Identity;
using eSaysay.Models.Entities;
using eSaysay.Models.ViewModels;
using System.Security.Claims;
using eSaysay.Services;

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Student")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly SecurityLogService _logService;

        public DashboardController(ILogger<DashboardController> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext context, SecurityLogService logService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logService = logService;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Fetch lessons
            var allLessons = await _context.Lessons.ToListAsync() ?? new List<Lesson>();

            // Fetch notifications (async fix)
            var notifications = await _context.Notification
                .Where(n => n.UserID == user.Id)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            // Logging
            if (!allLessons.Any())
            {
                _logger.LogWarning("No lessons were retrieved from the database.");
            }
            else
            {
                _logger.LogInformation($"Total Lessons Fetched: {allLessons.Count}");
                foreach (var lesson in allLessons)
                {
                    _logger.LogInformation($"Lesson ID: {lesson.LessonID}, Title: {lesson.Title}, Difficulty: {lesson.DifficultyLevel}");
                }
            }

            _logger.LogInformation($"Notifications Found: {notifications.Count}");

            // Categorize lessons
            var beginnerLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Beginner", StringComparison.OrdinalIgnoreCase)).ToList();
            var intermediateLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Intermediate", StringComparison.OrdinalIgnoreCase)).ToList();
            var advancedLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Advanced", StringComparison.OrdinalIgnoreCase)).ToList();

            _logger.LogInformation($"Beginner Lessons: {beginnerLessons.Count}, Intermediate Lessons: {intermediateLessons.Count}, Advanced Lessons: {advancedLessons.Count}");

            ViewBag.BeginnerLessons = beginnerLessons;
            ViewBag.IntermediateLessons = intermediateLessons;
            ViewBag.AdvancedLessons = advancedLessons;
            ViewBag.Notifications = notifications;

            // Pass data using ViewModel
            var viewModel = new DashboardViewModel
            {
                Lessons = allLessons,
                Notifications = notifications
            };

            return View("~/Views/User/Dashboard/Index.cshtml", viewModel);
        }

        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public IActionResult LessonDetails(int id)
        {
            var lesson = _context.Lessons.FirstOrDefault(l => l.LessonID == id);
            if (lesson == null)
            {
                return NotFound("Lesson not found.");
            }

            var exercises = _context.InteractiveExercises
                .Where(e => e.LessonID == id)
                .ToList();

            // Shuffle the exercises
            var random = new Random();
            exercises = exercises.OrderBy(x => random.Next()).ToList();

            ViewBag.Lesson = lesson;
            return View("~/Views/User/Dashboard/LessonDetails.cshtml", exercises);
        }


        public IActionResult StartExercise(int exerciseId)
        {
            var exercise = _context.InteractiveExercises.FirstOrDefault(e => e.ExerciseID == exerciseId);

            if (exercise == null)
            {
                return NotFound("Exercise not found.");
            }

            switch (exercise.ExerciseType)
            {
                case "Complete Translation":
                    return View("~/Views/User/Exercises/CompleteTranslation.cshtml", exercise);

                case "Correct Translation":
                    return View("~/Views/User/Exercises/CorrectTranslation.cshtml", exercise);

                case "Listening Exercise":
                    return View("~/Views/User/Exercises/ListeningExercise.cshtml", exercise);

                case "Pairing":
                    return View("~/Views/User/Exercises/PairingExercise.cshtml", exercise);

                default:
                    return NotFound($"Invalid exercise type: {exercise.ExerciseType}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Logs()
        {
            var logs = await _context.SecurityLog
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            return View("~/Views/User/Admin/Logs.cshtml", logs);
        }

        public async Task<IActionResult> UpdateProfile(IdentityUser updatedUser)
        {
            // Update user logic here...

            await _logService.LogEvent("User updated profile");

            return RedirectToAction("Profile");
        }

    }
}
