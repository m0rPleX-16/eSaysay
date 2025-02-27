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
            var allLessons = await _context.Lessons.ToListAsync();

            if (allLessons == null || allLessons.Count == 0)
            {
                _logger.LogWarning("No lessons were retrieved from the database.");
                allLessons = new List<Lesson>();
            }
            else
            {
                _logger.LogInformation($"Total Lessons Fetched: {allLessons.Count}");
                foreach (var lesson in allLessons)
                {
                    _logger.LogInformation($"Lesson ID: {lesson.LessonID}, Title: {lesson.Title}, Difficulty: {lesson.DifficultyLevel}");
                }
            }
    
            var beginnerLessons = allLessons
                .Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Beginner", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var intermediateLessons = allLessons
                .Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Intermediate", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var advancedLessons = allLessons
                .Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Advanced", StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation($"Beginner Lessons Found: {beginnerLessons.Count}");
            _logger.LogInformation($"Intermediate Lessons Found: {intermediateLessons.Count}");
            _logger.LogInformation($"Advanced Lessons Found: {advancedLessons.Count}");

            ViewBag.BeginnerLessons = beginnerLessons;
            ViewBag.IntermediateLessons = intermediateLessons;
            ViewBag.AdvancedLessons = advancedLessons;

            return View("~/Views/User/Dashboard/Index.cshtml", allLessons);
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
