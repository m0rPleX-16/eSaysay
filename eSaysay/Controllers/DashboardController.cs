using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using eSaysay.Models;
using System.Diagnostics;
using eSaysay.Models.Entities;
using eSaysay.Models.ViewModels;
using eSaysay.Data;
using eSaysay.Services;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Student")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly SecurityLogService _logService;

        public DashboardController(ILogger<DashboardController> logger, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context, SecurityLogService logService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logService = logService;
        }

        // Dashboard Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(user.LanguageExperience))
                return RedirectToAction("SelectLanguageExperience");

            var viewModel = await GetDashboardViewModel(user.Id);

            return View("~/Views/User/Dashboard/Index.cshtml", viewModel);
        }

        // Select Language Experience
        [HttpGet]
        public async Task<IActionResult> SelectLanguageExperience()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrEmpty(user.LanguageExperience))
                return RedirectToAction("Index");

            return View("~/Views/User/Dashboard/SelectLanguageExperience.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectLanguageExperience([Required] string experience)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            user.LanguageExperience = experience;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }

        // Mark Notification as Read
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if (notification == null || notification.UserID != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Lesson Details
        public IActionResult LessonDetails(int id)
        {
            var lesson = _context.Lessons.FirstOrDefault(l => l.LessonID == id);
            if (lesson == null)
            {
                return NotFound();
            }

            var exercises = _context.InteractiveExercises
                .Where(e => e.LessonID == id)
                .OrderBy(x => Guid.NewGuid()) // Shuffle the exercises for variety
                .ToList();

            ViewBag.Lesson = lesson;
            return View("~/Views/User/Dashboard/LessonDetails.cshtml", exercises);
        }

        // Save User Response
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUserResponse([FromForm] UserResponse userResponse, [FromForm] int TimeSpent)
        {
            if (userResponse == null || string.IsNullOrEmpty(userResponse.UserID))
            {
                _logger.LogWarning("Invalid response data: UserResponse is null or UserID is empty.");
                return BadRequest(new { message = "Invalid response data." });
            }

            if (TimeSpent < 0)
            {
                _logger.LogWarning("Invalid TimeSpent value: Cannot be negative.");
                return BadRequest(new { message = "Invalid TimeSpent value." });
            }

            // Ensure the user exists before proceeding
            if (!await UserExists(userResponse.UserID))
            {
                _logger.LogWarning($"UserID {userResponse.UserID} does not exist in AspNetUsers.");
                return BadRequest(new { message = "Invalid UserID." });
            }

            _logger.LogInformation($"UserID: {userResponse.UserID}");
            _logger.LogInformation($"ExerciseID: {userResponse.ExerciseID}");
            _logger.LogInformation($"TimeSpent: {TimeSpent}");

            try
            {
                var exercise = await _context.InteractiveExercises
                    .FirstOrDefaultAsync(e => e.ExerciseID == userResponse.ExerciseID);

                if (exercise == null)
                {
                    _logger.LogWarning($"Exercise with ID {userResponse.ExerciseID} not found.");
                    return BadRequest(new { message = "Invalid ExerciseID." });
                }

                userResponse.AttemptDate = DateTime.UtcNow;
                _context.UserResponse.Add(userResponse);
                await _context.SaveChangesAsync();

                await UpdateUserProgress(userResponse.UserID, exercise.LessonID, userResponse.IsCorrect, TimeSpent);
                await UpdateAnalytics(userResponse.UserID, exercise.LessonID, userResponse.IsCorrect, TimeSpent);
                await UpdateAdaptiveLearning(userResponse.UserID, exercise.LessonID);

                _logger.LogInformation("Response saved successfully.");
                return Json(new { success = true, message = "Response saved successfully." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while saving response: {ex.Message}");
                return StatusCode(500, new { message = "A database error occurred while saving your response." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving response: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while saving your response." });
            }
        }

        // Profile
        public async Task<IActionResult> ProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.LanguageExperience = user?.LanguageExperience ?? "Not Set";

            return View("~/Views/User/Dashboard/Profile.cshtml");
        }

        // Translator
        public IActionResult Translator()
        {
            return View("~/Views/User/Dashboard/Translator.cshtml");
        }

        // Practice Speech
        public IActionResult PracticeSpeech()
        {
            return View("~/Views/User/Dashboard/PracticeSpeech.cshtml");
        }

        // Start Exercise
        public IActionResult StartExercise(int exerciseId)
        {
            var exercise = _context.InteractiveExercises.FirstOrDefault(e => e.ExerciseID == exerciseId);

            if (exercise == null)
            {
                return NotFound("Exercise not found.");
            }

            return exercise.ExerciseType switch
            {
                "Complete Translation" => View("~/Views/User/Exercises/CompleteTranslation.cshtml", exercise),
                "Correct Translation" => View("~/Views/User/Exercises/CorrectTranslation.cshtml", exercise),
                "Listening Exercise" => View("~/Views/User/Exercises/ListeningExercise.cshtml", exercise),
                "Pairing" => View("~/Views/User/Exercises/PairingExercise.cshtml", exercise),
                _ => NotFound($"Invalid exercise type: {exercise.ExerciseType}"),
            };
        }

        // Success
        public IActionResult Success()
        {
            return View();
        }

        // Stats
        public async Task<IActionResult> Stats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var viewModel = await GetStatsViewModel(user.Id);

            return View("~/Views/User/Dashboard/Stats.cshtml", viewModel);
        }

        // Logs
        [HttpGet]
        public async Task<IActionResult> Logs()
        {
            var logs = await _context.SecurityLog
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            return View("~/Views/User/Admin/Logs.cshtml", logs);
        }

        // Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser updatedUser)
        {
            try
            {
                // Update user logic here...

                await _logService.LogEvent("User updated profile");

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating profile: {ex.Message}");
                return StatusCode(500, "An error occurred while updating your profile.");
            }
        }

        // Helper Methods
        private async Task<DashboardViewModel> GetDashboardViewModel(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var userExperienceLevel = user?.LanguageExperience ?? "Beginner";

            var allLessons = await _context.Lessons.ToListAsync() ?? new List<Lesson>();
            var notifications = await _context.Notification
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();
            var userProgress = await _context.UserProgress
                .Include(up => up.Lesson)
                .Where(up => up.UserID == userId)
                .ToListAsync() ?? new List<UserProgress>();
            var analytics = await _context.Analytics
                .FirstOrDefaultAsync(a => a.UserID == userId);
            var adaptiveLearning = await _context.AdaptiveLearning
                .FirstOrDefaultAsync(al => al.UserID == userId);

            var beginnerLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Beginner", StringComparison.OrdinalIgnoreCase)).ToList();
            var intermediateLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Intermediate", StringComparison.OrdinalIgnoreCase)).ToList();
            var advancedLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Advanced", StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.BeginnerLessons = beginnerLessons;
            ViewBag.IntermediateLessons = intermediateLessons;
            ViewBag.AdvancedLessons = advancedLessons;

            return new DashboardViewModel
            {
                Lessons = allLessons,
                Notifications = notifications,
                UserProgress = userProgress,
                Analytics = analytics,
                AdaptiveLearning = adaptiveLearning,
                UserExperienceLevel = userExperienceLevel 
            };
        }

        private async Task<DashboardViewModel> GetStatsViewModel(string userId)
        {
            var analytics = await _context.Analytics.FirstOrDefaultAsync(a => a.UserID == userId);
            var userProgress = await _context.UserProgress
                .Include(up => up.Lesson)
                .Where(up => up.UserID == userId)
                .ToListAsync() ?? new List<UserProgress>();
            var adaptiveLearning = await _context.AdaptiveLearning
                .FirstOrDefaultAsync(al => al.UserID == userId);

            return new DashboardViewModel
            {
                Analytics = analytics,
                UserProgress = userProgress,
                AdaptiveLearning = adaptiveLearning
            };
        }

        private async Task<bool> UserExists(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        private async Task UpdateUserProgress(string userId, int lessonId, bool isCorrect, int TimeSpent)
        {
            if (!await UserExists(userId))
            {
                _logger.LogWarning($"UserID {userId} does not exist in AspNetUsers.");
                return;
            }

            var progress = await _context.UserProgress
                .FirstOrDefaultAsync(p => p.UserID == userId && p.LessonID == lessonId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserID = userId,
                    LessonID = lessonId,
                    CompletionStatus = isCorrect ? "Completed" : "In Progress",
                    Score = isCorrect ? 100 : 0,
                    TimeSpent = TimeSpent,
                    LastAccessedDate = DateTime.UtcNow
                };
                _context.UserProgress.Add(progress);
            }
            else
            {
                progress.CompletionStatus = isCorrect ? "Completed" : "In Progress";
                progress.Score = (progress.Score + (isCorrect ? 100 : 0)) / 2; // Update average score
                progress.LastAccessedDate = DateTime.UtcNow;
                progress.TimeSpent += TimeSpent;
            }
            await _context.SaveChangesAsync();
        }

        private async Task UpdateAnalytics(string userId, int lessonId, bool isCorrect, int TimeSpent)
        {
            if (!await UserExists(userId))
            {
                _logger.LogWarning($"UserID {userId} does not exist in AspNetUsers.");
                return;
            }

            var analytics = await _context.Analytics
                .FirstOrDefaultAsync(a => a.UserID == userId && a.LessonCompleted == lessonId);

            if (analytics == null)
            {
                analytics = new Analytics
                {
                    UserID = userId,
                    Date = DateTime.UtcNow,
                    LessonCompleted = lessonId,
                    AverageScore = isCorrect ? 100 : 0,
                    TimeSpent = TimeSpent
                };
                _context.Analytics.Add(analytics);
            }
            else
            {
                analytics.AverageScore = (analytics.AverageScore + (isCorrect ? 100 : 0)) / 2; // Update average score
                analytics.Date = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        private async Task UpdateAdaptiveLearning(string userId, int lessonId)
        {
            // Ensure the user exists before proceeding
            if (!await UserExists(userId))
            {
                _logger.LogWarning($"UserID {userId} does not exist in AspNetUsers.");
                return;
            }

            var adaptiveLearning = await _context.AdaptiveLearning
                .FirstOrDefaultAsync(al => al.UserID == userId);

            if (adaptiveLearning == null)
            {
                adaptiveLearning = new AdaptiveLearning
                {
                    UserID = userId,
                    CurrentLevel = 1, // Starting level
                    RecommendedLessons = new List<int> { lessonId }, // Example recommendation
                    LastUpdated = DateTime.UtcNow
                };
                _context.AdaptiveLearning.Add(adaptiveLearning);
            }
            else
            {
                // Update the current level and recommended lessons based on user performance
                adaptiveLearning.CurrentLevel += 1; // Example logic
                adaptiveLearning.RecommendedLessons.Add(lessonId); // Add the completed lesson to recommendations
                adaptiveLearning.LastUpdated = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }
}
