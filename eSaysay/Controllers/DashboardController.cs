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

            // Fetch all necessary data
            var allLessons = await _context.Lessons.ToListAsync() ?? new List<Lesson>();
            var notifications = await _context.Notification
                .Where(n => n.UserID == user.Id)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();
            var userProgress = await _context.UserProgress
                .Include(up => up.Lesson)
                .Where(up => up.UserID == user.Id)
                .ToListAsync() ?? new List<UserProgress>();

            _logger.LogInformation($"UserProgress Records (Before Passing to View): {userProgress?.Count ?? 0}");
            var analytics = await _context.Analytics
                .FirstOrDefaultAsync(a => a.UserID == user.Id);
            var userBadges = await _context.UserBadges
                .Include(ub => ub.Badge)
                .Where(ub => ub.UserID == user.Id)
                .ToListAsync();
            var adaptiveLearning = await _context.AdaptiveLearning
                .FirstOrDefaultAsync(al => al.UserID == user.Id);

            // Categorize lessons by difficulty
            var beginnerLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Beginner", StringComparison.OrdinalIgnoreCase)).ToList();
            var intermediateLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Intermediate", StringComparison.OrdinalIgnoreCase)).ToList();
            var advancedLessons = allLessons.Where(l => string.Equals(l.DifficultyLevel?.Trim(), "Advanced", StringComparison.OrdinalIgnoreCase)).ToList();

            // Logging for debugging
            _logger.LogInformation($"Total Lessons Fetched: {allLessons.Count}");
            _logger.LogInformation($"Notifications Found: {notifications.Count}");
            _logger.LogInformation($"UserProgress Records: {userProgress.Count}");
            _logger.LogInformation($"UserBadges: {userBadges.Count}");
            _logger.LogInformation($"AdaptiveLearning: {adaptiveLearning != null}");

            // Pass data to the view using ViewModel
            var viewModel = new DashboardViewModel
            {
                Lessons = allLessons,
                Notifications = notifications,
                UserProgress = userProgress,
                Analytics = analytics,
                UserBadges = userBadges,
                AdaptiveLearning = adaptiveLearning
            };

            // ViewBag for categorized lessons
            ViewBag.BeginnerLessons = beginnerLessons;
            ViewBag.IntermediateLessons = intermediateLessons;
            ViewBag.AdvancedLessons = advancedLessons;

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

            // Shuffle the exercises for variety
            var random = new Random();
            exercises = exercises.OrderBy(x => random.Next()).ToList();

            ViewBag.Lesson = lesson;
            return View("~/Views/User/Dashboard/LessonDetails.cshtml", exercises);
        }

        [HttpPost]
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

            // Check if the UserID exists in AspNetUsers
            var userExists = _context.Users.Any(u => u.Id == userResponse.UserID);
            if (!userExists)
            {
                _logger.LogWarning($"UserID {userResponse.UserID} does not exist in AspNetUsers.");
                return BadRequest(new { message = "Invalid UserID." });
            }

            _logger.LogInformation($"UserID: {userResponse.UserID}");
            _logger.LogInformation($"ExerciseID: {userResponse.ExerciseID}");
            _logger.LogInformation($"UserAnswer: {userResponse.UserAnswer}");
            _logger.LogInformation($"IsCorrect: {userResponse.IsCorrect}");
            _logger.LogInformation($"TimeSpent: {TimeSpent}");

            try
            {
                // Fetch the exercise to get the LessonID
                var exercise = await _context.InteractiveExercises
                    .FirstOrDefaultAsync(e => e.ExerciseID == userResponse.ExerciseID);

                if (exercise == null)
                {
                    _logger.LogWarning($"Exercise with ID {userResponse.ExerciseID} not found.");
                    return BadRequest(new { message = "Invalid ExerciseID." });
                }

                // Save the user response
                userResponse.AttemptDate = DateTime.UtcNow;
                _context.UserResponse.Add(userResponse);
                await _context.SaveChangesAsync();

                // Update UserProgress with time tracking
                await UpdateUserProgress(userResponse.UserID, exercise.LessonID, userResponse.IsCorrect, TimeSpent);

                // Update Analytics
                await UpdateAnalytics(userResponse.UserID, exercise.LessonID, userResponse.IsCorrect);

                // Check and assign badges
                await CheckAndAssignBadges(userResponse.UserID);

                // Update Adaptive Learning
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
        private async Task UpdateUserProgress(string userId, int lessonId, bool isCorrect, int TimeSpent)
        {
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

        private async Task UpdateAnalytics(string userId, int lessonId, bool isCorrect)
        {
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
                    TimeSpent = 0 // You can track time spent using a timer
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

        private async Task CheckAndAssignBadges(string userId)
        {
            // Example: Assign a badge if the user completes 5 lessons
            var completedLessons = await _context.UserProgress
                .CountAsync(up => up.UserID == userId && up.CompletionStatus == "Completed");

            if (completedLessons >= 5)
            {
                var badge = await _context.Badges.FirstOrDefaultAsync(b => b.BadgeID == 1); // Example badge ID
                if (badge != null)
                {
                    var userBadge = new UserBadge
                    {
                        UserID = userId,
                        BadgeID = badge.BadgeID,
                        DateEarned = DateTime.UtcNow
                    };
                    _context.UserBadges.Add(userBadge);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public IActionResult Profile()
        {
            return View("~/Views/User/Dashboard/Profile.cshtml");
        }

        public IActionResult Translator()
        {
            return View("~/Views/User/Dashboard/Translator.cshtml");
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

        public IActionResult Success()
        {
            return View();
        }
        public async Task<IActionResult> Stats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var analytics = await _context.Analytics.FirstOrDefaultAsync(a => a.UserID == user.Id);
            var userProgress = await _context.UserProgress
                .Include(up => up.Lesson)
                .Where(up => up.UserID == user.Id)
                .ToListAsync() ?? new List<UserProgress>();
            var userBadges = await _context.UserBadges
                .Include(ub => ub.Badge)
                .Where(ub => ub.UserID == user.Id)
                .ToListAsync();
            var adaptiveLearning = await _context.AdaptiveLearning
                .FirstOrDefaultAsync(al => al.UserID == user.Id);

            var viewModel = new DashboardViewModel
            {
                Analytics = analytics,
                UserProgress = userProgress,
                UserBadges = userBadges,
                AdaptiveLearning = adaptiveLearning
            };

            return View("~/Views/User/Dashboard/Stats.cshtml", viewModel);
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