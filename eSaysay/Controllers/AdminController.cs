using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using eSaysay.Models;
using System.Diagnostics;
using eSaysay.Models.Entities;
using eSaysay.Data;
using eSaysay.Services;
using System.Text.Json;
using System.Net.Http;
using eSaysay.Views.User.Admin;
using eSaysay.Models.ViewModels;

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly SecurityLogService _logService;
        private readonly HttpClient _httpClient;
        public AdminController(ILogger<AdminController> logger,
                               UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               ApplicationDbContext context,
                               SecurityLogService logService,
                               HttpClient httpClient)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logService = logService;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalLessons = await _context.Lessons.CountAsync();
            var completedLessons = await _context.UserProgress.CountAsync(up => up.CompletionStatus == "Completed");
            var avgScore = await _context.Analytics.AverageAsync(a => (double?)a.AverageScore) ?? 0;
            var totalTimeSpent = await _context.Analytics.SumAsync(a => a.TimeSpent);

            var recentActivity = await _context.UserProgress
                .OrderByDescending(up => up.LastAccessedDate)
                .Take(5)
                .Select(up => new RecentActivityModel
                {
                    FullName = $"{up.User.FirstName} {up.User.LastName}",
                    LessonTitle = up.Lesson.Title,
                    CompletionStatus = up.CompletionStatus,
                    LastAccessedDate = up.LastAccessedDate
                }).ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                AverageScore = avgScore,
                TotalTimeSpent = totalTimeSpent,
                RecentActivity = recentActivity
            };

            return View("~/Views/User/Admin/Index.cshtml", model);
        }


        // GET: Admin/Language with pagination and search
        public async Task<IActionResult> Language(string searchTerm, int page = 1, int pageSize = 5)
        {
            var query = _context.Language
                .Where(l => !l.IsArchived) // Only fetch languages that are NOT archived
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.LanguageName.Contains(searchTerm));
            }

            int totalItems = await query.CountAsync();
            var languages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View("~/Views/User/Admin/Language.cshtml", languages);
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> FilterLanguages(string searchTerm, int page = 1, int pageSize = 5)
        {
            var query = _context.Language
                .Where(l => !l.IsArchived) // Exclude archived languages
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.LanguageName.Contains(searchTerm));
            }

            int totalItems = await query.CountAsync();
            var languages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return PartialView("~/Views/User/Admin/Partial/_LanguageTablePartial.cshtml", languages);
        }


        // POST: Admin/AddLanguage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLanguage(Language language)
        {
            if (ModelState.IsValid)
            {
                _context.Language.Add(language);
                await _context.SaveChangesAsync();
                await _logService.LogEvent($"Added new language: {language.LanguageName}");
            }
            return RedirectToAction("Language");
        }

        // POST: Admin/EditLanguage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLanguage(Language language)
        {
            if (ModelState.IsValid)
            {
                _context.Language.Update(language);
                await _context.SaveChangesAsync();
                await _logService.LogEvent($"Updated language: {language.LanguageName}");
            }
            return RedirectToAction("Language");
        }

        // GET: Admin/ArchivedLanguage
        public async Task<IActionResult> ArchivedLanguage(string search, int page = 1, int pageSize = 5)
        {
            // Fetch only archived languages
            var archivedLanguagesQuery = _context.Language
                .Where(l => l.IsArchived)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                archivedLanguagesQuery = archivedLanguagesQuery
                    .Where(l => EF.Functions.Like(l.LanguageName, $"%{search}%"));
            }

            // Pagination
            int totalLanguages = await archivedLanguagesQuery.CountAsync();
            var archivedLanguages = await archivedLanguagesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass data to the view
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalLanguages / pageSize);

            return View("~/Views/User/Admin/Shared/ArchivedLanguage.cshtml", archivedLanguages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveLanguage(int LanguageID)
        {
            var language = await _context.Language.FindAsync(LanguageID);
            if (language != null)
            {
                language.IsArchived = true; // Soft delete
                _context.Language.Update(language);
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Archived language: {language.LanguageName}");
            }
            return RedirectToAction("Language");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreLanguage(int LanguageID)
        {
            var language = await _context.Language.FindAsync(LanguageID);
            if (language != null)
            {
                language.IsArchived = false; // Restore
                _context.Language.Update(language);
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Restored language: {language.LanguageName}");
            }
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLanguagePermanently(int LanguageID)
        {
            var language = await _context.Language.FindAsync(LanguageID);
            if (language != null)
            {
                _context.Language.Remove(language);
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Permanently deleted language: {language.LanguageName}");
            }
            return Ok();
        }
        public IActionResult Analytics()
        {
            try
            {
                var analyticsData = _context.Analytics
                    .GroupBy(a => a.Date.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalLessonsCompleted = g.Count(),
                        AvgScore = g.Average(a => a.AverageScore),
                        TotalTimeSpent = g.Sum(a => a.TimeSpent)
                    })
                    .OrderBy(a => a.Date)
                    .ToList();

                if (!analyticsData.Any())
                {
                    _logger.LogWarning("[Analytics] No analytics data available.");
                    return View("~/Views/User/Admin/Analytics.cshtml", new Models.ViewModels.AnalyticsModel
                    {
                        AnalyticsDates = new List<string>(),
                        LessonsCompleted = new List<int>(),
                        AnalyticsScores = new List<double>(),
                        AnalyticsTimeSpent = new List<int>(),
                        TotalUsers = _context.Users.Count(),
                        AvgScore = 0,
                        TotalTimeSpent = 0,
                        TotalLessonsCompleted = 0
                    });
                }

                var model = new Models.ViewModels.AnalyticsModel
                {
                    AnalyticsDates = analyticsData.Select(a => a.Date.ToString("yyyy-MM-dd")).ToList(),
                    LessonsCompleted = analyticsData.Select(a => a.TotalLessonsCompleted).ToList(),
                    AnalyticsScores = analyticsData.Select(a => a.AvgScore).ToList(),
                    AnalyticsTimeSpent = analyticsData.Select(a => a.TotalTimeSpent).ToList(),
                    TotalUsers = _context.Users.Count(),
                    AvgScore = _context.Analytics.Any() ? _context.Analytics.Average(a => a.AverageScore) : 0,
                    TotalTimeSpent = _context.Analytics.Any() ? _context.Analytics.Sum(a => a.TimeSpent) : 0,
                    TotalLessonsCompleted = _context.Analytics.Any() ? _context.Analytics.Count() : 0
                };

                return View("~/Views/User/Admin/Analytics.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Analytics] Error fetching analytics data: {ex.Message}");
                return View("~/Views/User/Admin/Analytics.cshtml", new Models.ViewModels.AnalyticsModel
                {
                    AnalyticsDates = new List<string>(),
                    LessonsCompleted = new List<int>(),
                    AnalyticsScores = new List<double>(),
                    AnalyticsTimeSpent = new List<int>(),
                    TotalUsers = _context.Users.Count(),
                    AvgScore = 0,
                    TotalTimeSpent = 0,
                    TotalLessonsCompleted = 0
                });
            }
        }

        // GET: Admin/Exercises
        public IActionResult Exercises(string searchTerm, int page = 1, int pageSize = 5)
        {
            

            var query = _context.InteractiveExercises
                .Include(e => e.Lesson)
                .Where(e => !e.IsArchived);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => 
                e.Content.Contains(searchTerm) ||
                e.ExerciseType.Contains(searchTerm) ||
                e.DifficultyLevel.Contains(searchTerm));
            }

            var totalRecords = query.Count();
            var exercises = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Lessons = _context.Lessons.ToList();

            return View("~/Views/User/Admin/Exercises.cshtml", exercises);
        }

        public IActionResult FilterExercises(string searchTerm, int page = 1, int pageSize = 5)
        {
            var query = _context.InteractiveExercises
                .Include(e => e.Lesson)
                .Where(e => !e.IsArchived);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.Content.Contains(searchTerm) ||
                    e.ExerciseType.Contains(searchTerm) ||
                    e.DifficultyLevel.Contains(searchTerm));
            }

            var totalRecords = query.Count();
            var exercises = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.SearchTerm = searchTerm;

            return PartialView("~/Views/User/Admin/Partial/_ExercisesTablePartial.cshtml", exercises);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExercise(InteractiveExercise exercise)
        {
            _logger.LogInformation($"Creating exercise: {exercise.ExerciseType}, Lesson ID: {exercise.LessonID}");

            // Ensure LessonID is valid before proceeding
            if (exercise.LessonID == 0)
            {
                _logger.LogWarning("Error: LessonID is required.");
                ModelState.AddModelError("LessonID", "The Lesson field is required.");
                return BadRequest(ModelState);
            }

            // Fetch Lesson from database
            var lesson = await _context.Lessons.FindAsync(exercise.LessonID);
            if (lesson == null)
            {
                _logger.LogWarning("Error: Lesson not found.");
                ModelState.AddModelError("LessonID", "Invalid Lesson ID.");
                return BadRequest(ModelState);
            }

            // Assign Lesson object explicitly
            exercise.Lesson = lesson;

            // Translate Content
            var translatedText = await TranslateToKorean(exercise.Content);
            _logger.LogInformation($"Translation result: {translatedText}");

            if (string.IsNullOrWhiteSpace(translatedText))
            {
                _logger.LogWarning("Error: Translation failed.");
                ModelState.AddModelError("ContentTranslate", "Translation failed.");
                return BadRequest(ModelState);
            }

            // Assign translated text before saving
            exercise.ContentTranslate = translatedText;

            // Validate ModelState after setting all required properties
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        _logger.LogWarning($"Model Error - {error.Key}: {subError.ErrorMessage}");
                    }
                }
                return BadRequest(ModelState);
            }

            // Process AnswerChoices
            if (!string.IsNullOrWhiteSpace(exercise.AnswerChoices))
            {
                if (!IsValidAnswerChoices(exercise.AnswerChoices))
                {
                    _logger.LogWarning("Error: AnswerChoices is not in a valid format.");
                    return BadRequest("Invalid format for AnswerChoices.");
                }
                exercise.AnswerChoices = FormatAnswerChoices(exercise.AnswerChoices);
            }
            else
            {
                exercise.AnswerChoices = null;
            }

            exercise.Hint = string.IsNullOrWhiteSpace(exercise.Hint) ? null : exercise.Hint;

            // Save to database
            _context.InteractiveExercises.Add(exercise);
            await _context.SaveChangesAsync();
            await _logService.LogEvent($"Created new exercise: {exercise.ExerciseType}");

            return RedirectToAction("Exercises");
        }

        // POST: Admin/EditExercise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExercise(InteractiveExercise exercise)
        {
            var existingExercise = await _context.InteractiveExercises.FirstOrDefaultAsync(e => e.ExerciseID == exercise.ExerciseID);

            if (existingExercise == null)
            {
                _logger.LogWarning($"Exercise with ID {exercise.ExerciseID} not found.");
                return NotFound($"Exercise with ID {exercise.ExerciseID} not found.");
            }

            existingExercise.ExerciseType = exercise.ExerciseType;
            existingExercise.Content = exercise.Content;
            existingExercise.ContentTranslate = await TranslateToKorean(exercise.Content);
            existingExercise.CorrectAnswer = exercise.CorrectAnswer;
            existingExercise.DifficultyLevel = exercise.DifficultyLevel;
            existingExercise.LessonID = exercise.LessonID;

            if (!string.IsNullOrWhiteSpace(exercise.AnswerChoices))
            {
                if (!IsValidAnswerChoices(exercise.AnswerChoices))
                {
                    _logger.LogWarning("Error: AnswerChoices is not in a valid format.");
                    return BadRequest("Invalid format for AnswerChoices.");
                }
                existingExercise.AnswerChoices = FormatAnswerChoices(exercise.AnswerChoices);
            }
            else
            {
                existingExercise.AnswerChoices = null;
            }

            existingExercise.Hint = string.IsNullOrWhiteSpace(exercise.Hint) ? null : exercise.Hint;

            await _context.SaveChangesAsync();
            await _logService.LogEvent($"Updated exercise: {exercise.ExerciseType}");
            _logger.LogInformation($"Exercise updated: {exercise.ExerciseType}");

            return RedirectToAction("Exercises");
        }

        // Helper method to validate AnswerChoices format
        private bool IsValidAnswerChoices(string answerChoices)
        {
            try
            {
                JsonDocument.Parse(answerChoices);
                return true;
            }
            catch (JsonException)
            {
                var values = answerChoices.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return values.Length > 0;
            }
        }

        private string FormatAnswerChoices(string answerChoices)
        {
            if (IsValidAnswerChoices(answerChoices))
            {
                try
                {
                    // If the input is already valid JSON, parse it and convert to comma-separated string
                    var jsonArray = JsonDocument.Parse(answerChoices).RootElement;
                    var values = jsonArray.EnumerateArray().Select(e => e.GetString()).ToList();
                    return string.Join(",", values);
                }
                catch (JsonException)
                {
                    // If the input is not JSON, assume it's a comma-separated string
                    var values = answerChoices.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return string.Join(",", values);
                }
            }
            return null;
        }
        // Helper method to translate content to Korean
        private async Task<string> TranslateToKorean(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=en|ko";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Translation API error: {response.StatusCode}");
                return text;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse);

            if (jsonDoc.RootElement.TryGetProperty("responseData", out var responseData) &&
                responseData.TryGetProperty("translatedText", out var translatedText))
            {
                return translatedText.GetString() ?? text;
            }

            return text;
        }
        // POST: Admin/ArchiveExercise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveExercise(int ExerciseID)
        {
            var exercise = await _context.InteractiveExercises.FindAsync(ExerciseID);
            if (exercise != null)
            {
                exercise.IsArchived = true; // Mark as archived instead of deleting
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Archived exercise ID: {ExerciseID}");
            }
            return RedirectToAction("Exercises");
        }

        // GET: Admin/ArchivedExercises with pagination and search
        public async Task<IActionResult> ArchivedExercises(string search, int page = 1, int pageSize = 5)
        {
            var archivedExercises = await _context.InteractiveExercises
                .Include(e => e.Lesson)
                .Where(e => e.IsArchived) // ✅ Only archived exercises
                .ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                archivedExercises = archivedExercises
                    .Where(e => e.Content.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            int totalExercises = archivedExercises.Count;
            archivedExercises = archivedExercises.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalExercises / pageSize);

            return View("~/Views/User/Admin/Shared/ArchivedExercises.cshtml", archivedExercises);
        }

        // POST: Admin/RestoreExercise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreExercise(int ExerciseID)
        {
            var exercise = await _context.InteractiveExercises.FindAsync(ExerciseID);
            if (exercise != null)
            {
                exercise.IsArchived = false;
                _context.InteractiveExercises.Update(exercise);
                await _context.SaveChangesAsync();
                await _logService.LogEvent($"Restored exercise: {exercise.Content}");
            }
            return RedirectToAction("ArchivedExercises");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExercisePermanently(int ExerciseID)
        {
            var exercise = await _context.InteractiveExercises.FindAsync(ExerciseID);
            if (exercise != null)
            {
                _context.InteractiveExercises.Remove(exercise);
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Permanently deleted exercise: {exercise.Content}");
            }
            return Ok();
        }
        public IActionResult Lessons(string searchQuery, int page = 1, int pageSize = 5)
        {
            var lessonsQuery = _context.Lessons
                .Include(l => l.Language)
                .Where(l => !l.IsArchived) // Ensure only non-archived lessons are fetched
                .AsQueryable();

            var languages = _context.Language.ToList();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                lessonsQuery = lessonsQuery.Where(l => l.Title.Contains(searchQuery)
                || l.Language.LanguageName.Contains(searchQuery)
                || l.LessonType.Contains(searchQuery));
            }

            int totalLessons = lessonsQuery.Count();
            var lessons = lessonsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Languages = languages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalLessons / (double)pageSize);
            ViewBag.SearchQuery = searchQuery;

            return View("~/Views/User/Admin/Lessons.cshtml", lessons);
        }

        public IActionResult FilterLessons(string searchQuery, int page = 1, int pageSize = 5)
        {
            try
            {
                var lessonsQuery = _context.Lessons
                    .Include(l => l.Language)
                    .Where(l => !l.IsArchived) // Add this to match your partial view
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    lessonsQuery = lessonsQuery.Where(l =>
                        l.Title.Contains(searchQuery) ||
                        l.LessonType.Contains(searchQuery) ||
                        l.DifficultyLevel.Contains(searchQuery) ||
                        l.Language.LanguageName.Contains(searchQuery)
                    );
                }

                int totalLessons = lessonsQuery.Count();
                var lessons = lessonsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.Languages = _context.Language.ToList();
                ViewBag.TotalPages = (int)Math.Ceiling(totalLessons / (double)pageSize);

                return PartialView("~/Views/User/Admin/Partial/_LessonsTablePartial.cshtml", lessons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // POST: Admin/CreateLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(Lesson lesson)
        {
            _logger.LogInformation($"Creating lesson: {lesson.Title}, Language ID: {lesson.LanguageID}, Type: {lesson.LessonType}");

            if (lesson == null || string.IsNullOrWhiteSpace(lesson.Title))
            {
                _logger.LogInformation("Error: Lesson data is missing or invalid.");
                return BadRequest("Invalid lesson data.");
            }

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            await _logService.LogEvent($"Created lesson: {lesson.Title}");
            return RedirectToAction("Lessons");
        }

        // POST: Admin/EditLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(Lesson lesson)
        {
            var existingLesson = _context.Lessons.Find(lesson.LessonID);
            if (existingLesson != null)
            {
                existingLesson.Title = lesson.Title;
                existingLesson.Description = lesson.Description;
                existingLesson.LessonType = lesson.LessonType;
                existingLesson.DifficultyLevel = lesson.DifficultyLevel;
                existingLesson.LanguageID = lesson.LanguageID;

                await _context.SaveChangesAsync();
                await _logService.LogEvent($"Updated lesson: {lesson.Title}");
                _logger.LogInformation($"Lesson updated: {lesson.Title}");
            }
            else
            {
                _logger.LogInformation("Lesson not found!");
            }

            return RedirectToAction("Lessons");
        }

        // POST: Admin/ArchiveLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveLesson(int LessonID)
        {
            var lesson = await _context.Lessons.FindAsync(LessonID);
            if (lesson != null)
            {
                lesson.IsArchived = true; // Mark as archived instead of deleting
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Archived lesson: {lesson.Title}");
            }
            return RedirectToAction("Lessons");
        }

        // GET: Admin/ArchivedLessons (View Archived Lessons)
        public async Task<IActionResult> ArchivedLessons(string searchQuery, int page = 1, int pageSize = 5)
        {
            var archivedLessonsQuery = _context.Lessons
                .Include(l => l.Language)
                .Where(l => l.IsArchived) // Only fetch archived lessons
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                archivedLessonsQuery = archivedLessonsQuery.Where(l => l.Title.Contains(searchQuery)
                 || l.Language.LanguageName.Contains(searchQuery)
                 || l.LessonType.Contains(searchQuery));
            }

            int totalArchivedLessons = await archivedLessonsQuery.CountAsync();
            var archivedLessons = await archivedLessonsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalArchivedLessons / (double)pageSize);
            ViewBag.SearchQuery = searchQuery;

            return View("~/Views/User/Admin/Shared/ArchivedLessons.cshtml", archivedLessons);
        }

        // POST: Admin/RestoreLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreLesson(int LessonID)
        {
            var lesson = await _context.Lessons.FindAsync(LessonID);
            if (lesson != null)
            {
                lesson.IsArchived = false; // Unarchive the lesson
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Restored lesson: {lesson.Title}");
            }
            return RedirectToAction("ArchivedLessons");
        }

        // POST: Admin/DeleteLessonPermanent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLessonPermanent(int LessonID)
        {
            var lesson = await _context.Lessons.FindAsync(LessonID);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson); // Permanently delete the lesson
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Permanently deleted lesson: {lesson.Title}");
            }
            return RedirectToAction("ArchivedLessons");
        }

        [HttpGet]
        public async Task<IActionResult> Logs(string searchQuery, int page = 1, int pageSize = 10)
        {
            var logsQuery = _context.SecurityLog
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                logsQuery = logsQuery.Where(log =>
                    (log.User != null && log.User.Email.Contains(searchQuery)) ||
                    log.Event.Contains(searchQuery));
            }

            var totalLogs = await logsQuery.CountAsync();
            var logs = await logsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);
            ViewBag.SearchQuery = searchQuery;

            return View("~/Views/User/Admin/Logs.cshtml", logs);
        }
        public async Task<IActionResult> FilterLogs(string searchQuery, int page = 1, int pageSize = 10)
        {
            var logsQuery = _context.SecurityLog
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                logsQuery = logsQuery.Where(log =>
                    (log.User != null && log.User.Email.Contains(searchQuery)) ||
                    log.Event.Contains(searchQuery));
            }

            var totalLogs = await logsQuery.CountAsync();
            var logs = await logsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

            return PartialView("~/Views/User/Admin/Partial/_LogsTablePartial.cshtml", logs);
        }

        public async Task<IActionResult> Progress()
        {
            try
            {
                var progressData = await _context.UserProgress
                    .Include(up => up.User)
                    .Include(up => up.Lesson)
                    .ToListAsync();

                if (!progressData.Any())
                {
                    _logger.LogWarning("[Progress] No progress data found.");
                    return View("~/Views/User/Admin/Progress.cshtml", new ProgressViewModel
                    {
                        TotalUsers = await _context.Users.CountAsync(),
                        TotalLessons = await _context.Lessons.CountAsync(),
                        CompletedLessons = 0,
                        InProgressLessons = 0,
                        NotStartedLessons = 0,
                        AverageScore = 0,
                        ProgressData = new List<ProgressDetailViewModel>()
                    });
                }

                var viewModel = new ProgressViewModel
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalLessons = await _context.Lessons.CountAsync(),
                    CompletedLessons = progressData.Count(p => p.CompletionStatus == "Completed"),
                    InProgressLessons = progressData.Count(p => p.CompletionStatus == "In Progress"),
                    NotStartedLessons = progressData.Count(p => p.CompletionStatus == "Not Started"),
                    AverageScore = progressData.Any(p => p.Score.HasValue) ? progressData.Average(p => p.Score ?? 0) : 0,
                    ProgressData = progressData.Select(p => new ProgressDetailViewModel
                    {
                        UserName = p.User?.FirstName ?? "Student",
                        LessonName = p.Lesson?.Title ?? "Unknown Lesson",
                        CompletionStatus = p.CompletionStatus ?? "Not Started",
                        Score = p.Score ?? 0, 
                        TimeSpent = p.TimeSpent,
                        LastAccessedDate = p.LastAccessedDate
                    }).ToList()
                };

                return View("~/Views/User/Admin/Progress.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Progress] Error fetching progress data: {ex.Message}");
                return View("~/Views/User/Admin/Progress.cshtml", new ProgressViewModel
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalLessons = await _context.Lessons.CountAsync(),
                    CompletedLessons = 0,
                    InProgressLessons = 0,
                    NotStartedLessons = 0,
                    AverageScore = 0,
                    ProgressData = new List<ProgressDetailViewModel>()
                });
            }
        }

        // ✅ Fetch Active Students
        public async Task<IActionResult> Students(string search, int page = 1, int pageSize = 5)
        {
            var studentIds = (await _userManager.GetUsersInRoleAsync("Student")).Select(s => s.Id);

            var query = _userManager.Users
                .Where(u => studentIds.Contains(u.Id) && !u.IsArchived)
                .AsQueryable();

            // Apply search filter (Email, First Name, Last Name)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.Gender.Contains(search));
            }

            int totalStudents = await query.CountAsync();
            var students = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalStudents / pageSize);

            return View("~/Views/User/Admin/Students.cshtml", students);
        }

        // to search students on searchbar
        public async Task<IActionResult> FilterStudents(string search, int page = 1, int pageSize = 5)
        {
            var studentIds = (await _userManager.GetUsersInRoleAsync("Student")).Select(s => s.Id);

            var query = _userManager.Users
                .Where(u => studentIds.Contains(u.Id) && !u.IsArchived)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                u.Gender.Contains(search));
            }

            int totalStudents = await query.CountAsync();
            var students = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalStudents / pageSize);

            return PartialView("~/Views/User/Admin/Partial/_StudentsTablePartial.cshtml", students);
        }


        // ✅ Fetch Archived Students
        public async Task<IActionResult> ArchivedStudents(string search, int page = 1, int pageSize = 5)
            {
                var allUsers = await _userManager.Users.Where(u => u.IsArchived).ToListAsync(); // ✅ Only archived users

                var students = new List<ApplicationUser>();
                foreach (var user in allUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, "Student"))
                    {
                        students.Add(user);
                    }
                }

                if (!string.IsNullOrEmpty(search))
                {
                    students = students.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                int totalStudents = students.Count;
                students = students.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.Search = search;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalStudents / pageSize);

                return View("~/Views/User/Admin/Shared/ArchivedStudents.cshtml", students);
            }

        // ✅ Archive Student
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            user.IsArchived = true;  // ✅ Soft-delete
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _logService.LogEvent($"Archived student ID: {Id}");
                return RedirectToAction("Students");
            }

            return BadRequest("Failed to archive user.");
        }

        // ✅ Restore Student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStudent(string Id)
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

            user.IsArchived = false;  // ✅ Restore user
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _logService.LogEvent($"Restored student ID: {Id}");
                return RedirectToAction("ArchivedStudents");
            }

            return BadRequest("Failed to restore user.");
        }

        // ✅ Delete Student Permanently
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentPermanently(string Id)
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

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _logService.LogEvent($"Permanently deleted student ID: {Id}");
                return RedirectToAction("ArchivedStudents");
            }

            return BadRequest("Failed to delete user permanently.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
