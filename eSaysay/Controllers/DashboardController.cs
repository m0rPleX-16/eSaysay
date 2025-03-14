﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using eSaysay.Models;
using System.Diagnostics;
using eSaysay.Models.Entities;
using eSaysay.Models.ViewModels;
using eSaysay.Data;
using eSaysay.Services;
using System.ComponentModel.DataAnnotations;
using System;

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

        public async Task<IActionResult> LessonDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonID == id);
            if (lesson == null) return NotFound();

            var userId = user.Id;

            // Get all exercises in the lesson
            var exercises = await _context.InteractiveExercises
                .Where(e => e.LessonID == id && !e.IsArchived)
                .Select(e => new
                {
                    Exercise = e,
                    LastAttempts = _context.UserResponse
                        .Where(ur => ur.UserID == userId && ur.ExerciseID == e.ExerciseID)
                        .OrderByDescending(ur => ur.AttemptDate)
                        .ToList()
                })
                .ToListAsync();

            // Get current time
            var now = DateTime.UtcNow;

            var exercisesToReview = exercises
                .Select(x => new
                {
                    x.Exercise,
                    CorrectAttempts = x.LastAttempts.Count(ur => ur.IsCorrect), // Count correct attempts
                    LastAttemptDate = x.LastAttempts.FirstOrDefault()?.AttemptDate ?? now
                })
                .Select(x => new
                {
                    x.Exercise,
                    NextReviewDate = x.CorrectAttempts == 0 ? now :
                        x.LastAttemptDate.AddDays(Math.Pow(2, x.CorrectAttempts)) // Apply spaced repetition interval
                })
                .OrderBy(x => x.NextReviewDate) // Prioritize exercises due for review
                .Select(x => x.Exercise)
                .ToList();

            var userProgress = await _context.UserProgress
                .FirstOrDefaultAsync(up => up.UserID == user.Id && up.LessonID == id);

            var completedExercises = await _context.UserResponse
                .Where(ur => ur.UserID == user.Id && ur.IsCorrect)
                .Select(ur => ur.ExerciseID)
                .Distinct()
                .ToListAsync();

            var startedExercises = await _context.UserResponse
                .Where(ur => ur.UserID == user.Id && exercises.Select(e => e.Exercise.ExerciseID).Contains(ur.ExerciseID))
                .Select(ur => ur.ExerciseID)
                .Distinct()
                .ToListAsync();

            // Categorizing exercises
            var notStartedExercises = exercises
                .Where(e => !startedExercises.Contains(e.Exercise.ExerciseID))
                .OrderBy(x => Guid.NewGuid()) // Shuffle Not Started exercises
                .Select(e => e.Exercise)
                .ToList();

            var inProgressExercises = exercises
                .Where(e => startedExercises.Contains(e.Exercise.ExerciseID) && !completedExercises.Contains(e.Exercise.ExerciseID))
                .OrderBy(x => Guid.NewGuid()) // Shuffle In Progress exercises
                .Select(e => e.Exercise)
                .ToList();

            var completedExercisesList = exercises
                .Where(e => completedExercises.Contains(e.Exercise.ExerciseID))
                .Select(e => e.Exercise)
                .ToList(); // Keep Completed exercises in their original order

            // Merging them into a single ordered list
            var orderedExercises = notStartedExercises.Concat(inProgressExercises).Concat(completedExercisesList).ToList();

            ViewBag.Lesson = lesson;
            ViewBag.StartedExercises = startedExercises ?? new List<int>();
            ViewBag.CompletedExercises = completedExercises;

            return View("~/Views/User/Dashboard/LessonDetails.cshtml", orderedExercises);
        }

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

            if (!await UserExists(userResponse.UserID))
            {
                _logger.LogWarning($"UserID {userResponse.UserID} does not exist.");
                return BadRequest(new { message = "Invalid UserID." });
            }

            try
            {
                var exercise = await _context.InteractiveExercises.FindAsync(userResponse.ExerciseID);
                if (exercise == null)
                {
                    _logger.LogWarning($"Exercise with ID {userResponse.ExerciseID} not found.");
                    return BadRequest(new { message = "Invalid ExerciseID." });
                }

                var correctAttempts = await _context.UserResponse
                    .Where(r => r.UserID == userResponse.UserID && r.ExerciseID == userResponse.ExerciseID && r.IsCorrect)
                    .CountAsync();

                var existingResponse = await _context.UserResponse
                    .OrderByDescending(r => r.AttemptDate)
                    .FirstOrDefaultAsync(r => r.UserID == userResponse.UserID && r.ExerciseID == userResponse.ExerciseID);

                if (existingResponse != null)
                {
                    existingResponse.IsCorrect = userResponse.IsCorrect;
                    existingResponse.AttemptDate = DateTime.UtcNow;
                    _context.UserResponse.Update(existingResponse);
                }
                else
                {
                    userResponse.AttemptDate = DateTime.UtcNow;
                    _context.UserResponse.Add(userResponse);
                }

                await _context.SaveChangesAsync();

                // ✅ Update user progress, analytics, and adaptive learning
                await UpdateUserProgress(userResponse.UserID, exercise.LessonID, userResponse.IsCorrect, TimeSpent);
                await UpdateAnalytics(userResponse.UserID, exercise.LessonID, userResponse.IsCorrect, TimeSpent);
                await UpdateAdaptiveLearning(userResponse.UserID, exercise.LessonID);

                _logger.LogInformation($"Response saved for User {userResponse.UserID}, Exercise {userResponse.ExerciseID}.");
                return Json(new { success = true, message = "Response saved successfully." });
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
                .FirstOrDefaultAsync(al => al.UserID == userId) ?? new AdaptiveLearning();

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
                .FirstOrDefaultAsync(al => al.UserID == userId) ?? new AdaptiveLearning();

            return new DashboardViewModel
            {
                Analytics = analytics,
                UserProgress = userProgress,
                AdaptiveLearning = adaptiveLearning
            };
        }

        private async Task UpdateUserProgress(string userId, int lessonId, bool isCorrect, int TimeSpent)
        {
            if (!await UserExists(userId)) return;

            var totalExercises = await _context.InteractiveExercises
    .Where(e => e.LessonID == lessonId && !e.IsArchived)
    .CountAsync();

            var completedExercises = await _context.UserResponse
                .Where(r => r.UserID == userId && r.Exercise.LessonID == lessonId && r.IsCorrect)
                .Select(r => r.ExerciseID)
                .Distinct()
                .CountAsync();

            var progress = await _context.UserProgress.FirstOrDefaultAsync(p => p.UserID == userId && p.LessonID == lessonId);
            bool wasAlreadyCompleted = progress?.CompletionStatus == "Completed";

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserID = userId,
                    LessonID = lessonId,
                    CompletionStatus = (completedExercises >= totalExercises) ? "Completed" : "In Progress",
                    Score = isCorrect ? 100 : 0,
                    TimeSpent = TimeSpent,
                    LastAccessedDate = DateTime.UtcNow
                };
                _context.UserProgress.Add(progress);
            }
            else
            {
                progress.CompletionStatus = (completedExercises >= totalExercises) ? "Completed" : "In Progress";
                progress.Score = (progress.Score + (isCorrect ? 100 : 0)) / 2;
                progress.LastAccessedDate = DateTime.UtcNow;
                progress.TimeSpent += TimeSpent;
            }

            await _context.SaveChangesAsync();

            if (progress.CompletionStatus == "Completed" && !wasAlreadyCompleted)
            {
                var lesson = await _context.Lessons.FindAsync(lessonId);
                await SendNotification(userId, $"Great job! You have completed the lesson: {lesson?.Title}.", "UTC");
            }

            await UpdateLanguageExperience(userId);
        }
        private async Task UpdateLanguageExperience(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"[UpdateLanguageExperience] User {userId} not found. Skipping.");
                return;
            }

            var beginnerLessons = await _context.Lessons
                .Where(l => l.DifficultyLevel == "Beginner")
                .Select(l => l.LessonID)
                .ToListAsync();

            var intermediateLessons = await _context.Lessons
                .Where(l => l.DifficultyLevel == "Intermediate")
                .Select(l => l.LessonID)
                .ToListAsync();

            var completedLessons = await _context.UserProgress
                .Where(up => up.UserID == userId && up.CompletionStatus == "Completed")
                .Select(up => up.LessonID)
                .ToListAsync();

            _logger.LogInformation($"[UpdateLanguageExperience] User {userId} has completed {completedLessons.Count} lessons.");

            string previousLevel = user.LanguageExperience;

            if (previousLevel == "Beginner" && beginnerLessons.All(l => completedLessons.Contains(l)))
            {
                _logger.LogInformation($"[UpdateLanguageExperience] Student {userId} completed all Beginner lessons. Unlocking Intermediate.");
                user.LanguageExperience = "Intermediate";
                await SendNotification(userId, "Congratulations! You have unlocked Intermediate lessons.", "UTC");
            }
            else if (previousLevel == "Intermediate" && intermediateLessons.All(l => completedLessons.Contains(l)))
            {
                _logger.LogInformation($"[UpdateLanguageExperience] Student {userId} completed all Intermediate lessons. Unlocking Advanced.");
                user.LanguageExperience = "Advanced";
                await SendNotification(userId, "Great work! You have unlocked Advanced lessons.", "UTC");
            }
            else
            {
                _logger.LogInformation($"[UpdateLanguageExperience] No changes for user {userId}. Current level: {user.LanguageExperience}");
            }

            await _context.SaveChangesAsync();
        }

        private async Task SendNotification(string userId, string message, string timeZone, string title = "System Notification")
        {
            _logger.LogInformation($"[SendNotification] Sending notification to user {userId}: {message}");
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);

            var notification = new Notification
            {
                UserID = userId,
                Title = title,
                Message = message,
                IsRead = false,
                DateCreated = localTime
            };

            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"[SendNotification] Notification saved to database for user {userId}.");
        }

        private async Task UpdateAnalytics(string userId, int lessonId, bool isCorrect, int TimeSpent)
        {
            if (!await UserExists(userId)) return;

            var analytics = await _context.Analytics.FirstOrDefaultAsync(a => a.UserID == userId && a.LessonCompleted == lessonId);
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
                analytics.AverageScore = (analytics.AverageScore + (isCorrect ? 100 : 0)) / 2;
                analytics.TimeSpent += TimeSpent; // Add new time to existing time
                analytics.Date = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        private async Task UpdateAdaptiveLearning(string userId, int lessonId)
        {
            if (!await UserExists(userId))
            {
                _logger.LogWarning($"[Adaptive Learning] User {userId} does not exist in Users table. Skipping update.");
                return;
            }

            // Debugging: Check if the user has an Adaptive Learning entry
            var adaptiveLearning = await _context.AdaptiveLearning.FirstOrDefaultAsync(al => al.UserID == userId);

            if (adaptiveLearning == null)
            {
                _logger.LogWarning($"[Adaptive Learning] No existing record found for user {userId} in AdaptiveLearning.");
                var existingUser = await _context.Users.FindAsync(userId);

                if (existingUser == null)
                {
                    _logger.LogError($"[Adaptive Learning] ERROR: User {userId} does not exist in the Users table!");
                    return;
                }

                _logger.LogInformation($"[Adaptive Learning] Creating a new entry for user {userId}.");

                adaptiveLearning = new AdaptiveLearning
                {
                    UserID = userId,
                    CurrentLevel = 1,
                    RecommendedLessons = new List<int> { lessonId },
                    LastUpdated = DateTime.UtcNow
                };

                _context.AdaptiveLearning.Add(adaptiveLearning);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"[Adaptive Learning] New entry created for user {userId}.");
            }
            else
            {
                _logger.LogInformation($"[Adaptive Learning] Updating Adaptive Learning for user {userId}. Previous level: {adaptiveLearning.CurrentLevel}");

                adaptiveLearning.CurrentLevel += 1;
                if (!adaptiveLearning.RecommendedLessons.Contains(lessonId))
                {
                    adaptiveLearning.RecommendedLessons.Add(lessonId);
                }
                adaptiveLearning.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"[Adaptive Learning] Updated Adaptive Learning for user {userId}. New level: {adaptiveLearning.CurrentLevel}");
            }
        }

        private async Task<bool> UserExists(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}