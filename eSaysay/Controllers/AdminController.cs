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

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly SecurityLogService _logService;
        public AdminController(ILogger<AdminController> logger,
                               UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               ApplicationDbContext context,
                               SecurityLogService logService) 
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logService = logService; 
        }


        public IActionResult Index()
        {
            return View("~/Views/User/Admin/Index.cshtml");
        }

        // GET: Admin/Language with pagination and search
        public async Task<IActionResult> Language(string searchTerm, int page = 1, int pageSize = 5)
        {
            var query = _context.Language.AsQueryable();

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
        public async Task<IActionResult> FilterLanguages(string searchTerm, int page = 1, int pageSize = 5)
        {
            var query = _context.Language.AsQueryable();

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

            return PartialView("~/Views/User/Admin/Partial/_LanguageTablePartial.cshtml", languages);
        }



        // POST: Admin/AddLanguage
        [HttpPost]
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

        // POST: Admin/DeleteLanguage
        [HttpPost]
        public async Task<IActionResult> DeleteLanguage(int LanguageID)
        {
            var language = _context.Language.Find(LanguageID);
            if (language != null)
            {
                _context.Language.Remove(language);
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Archived language: {language.LanguageName}");
            }
            return RedirectToAction("Language");
        }

        public IActionResult Analytics()
        {
            return View("~/Views/User/Admin/Analytics.cshtml");
        }

        // GET: Admin/Exercises
        public IActionResult Exercises()
        {
            var exercises = _context.InteractiveExercises.Include(e => e.Lesson).ToList();
            var lessons = _context.Lessons.ToList();

            _logger.LogInformation($"Lessons count: {lessons.Count}");

            if (!lessons.Any())
            {
                _logger.LogWarning("ViewBag.Lessons is empty even though the database has data!");
            }

            ViewBag.Lessons = lessons;
            return View("~/Views/User/Admin/Exercises.cshtml", exercises);
        }

        // POST: Admin/CreateExercise
        [HttpPost]
        public async Task<IActionResult> CreateExercise(InteractiveExercise exercise)
        {
            _logger.LogInformation($"Creating exercise: {exercise.ExerciseType}, Lesson ID: {exercise.LessonID}");

            if (exercise == null || string.IsNullOrWhiteSpace(exercise.ExerciseType))
            {
                _logger.LogInformation("Error: Exercise data is missing or invalid.");
                return BadRequest("Invalid exercise data.");
            }

            // Validate and parse AnswerChoices
            if (!string.IsNullOrWhiteSpace(exercise.AnswerChoices))
            {
                if (!IsValidAnswerChoices(exercise.AnswerChoices))
                {
                    _logger.LogInformation("Error: AnswerChoices is not in a valid format (expected JSON or comma-separated values).");
                    return BadRequest("Invalid format for AnswerChoices. Expected JSON or comma-separated values.");
                }
                exercise.AnswerChoices = FormatAnswerChoices(exercise.AnswerChoices); // Format the data
            }
            else
            {
                exercise.AnswerChoices = null; // Set to null if empty
            }

            // Handle Hint
            if (string.IsNullOrWhiteSpace(exercise.Hint))
            {
                exercise.Hint = null; // Set to null if empty
            }

            _context.InteractiveExercises.Add(exercise);
            await _context.SaveChangesAsync();
            await _logService.LogEvent($"Created new exercise: {exercise.ExerciseType}");

            return RedirectToAction("Exercises");
        }

        // POST: Admin/EditExercise
        [HttpPost]
        public async Task<IActionResult> EditExercise(InteractiveExercise exercise)
        {
            var existingExercise = _context.InteractiveExercises.Find(exercise.ExerciseID);
            if (existingExercise != null)
            {
                existingExercise.ExerciseType = exercise.ExerciseType;
                existingExercise.Content = exercise.Content;
                existingExercise.CorrectAnswer = exercise.CorrectAnswer;
                existingExercise.DifficultyLevel = exercise.DifficultyLevel;
                existingExercise.LessonID = exercise.LessonID;

                // Validate and parse AnswerChoices
                if (!string.IsNullOrWhiteSpace(exercise.AnswerChoices))
                {
                    if (!IsValidAnswerChoices(exercise.AnswerChoices))
                    {
                        _logger.LogInformation("Error: AnswerChoices is not in a valid format (expected JSON or comma-separated values).");
                        return BadRequest("Invalid format for AnswerChoices. Expected JSON or comma-separated values.");
                    }
                    existingExercise.AnswerChoices = FormatAnswerChoices(exercise.AnswerChoices); 
                }
                else
                {
                    existingExercise.AnswerChoices = null; 
                }

                // Handle Hint
                existingExercise.Hint = string.IsNullOrWhiteSpace(exercise.Hint) ? null : exercise.Hint;

                await _context.SaveChangesAsync();
                await _logService.LogEvent($"Updated exercise: {exercise.ExerciseType}");
                _logger.LogInformation($"Exercise updated: {exercise.ExerciseType}");
            }
            else
            {
                _logger.LogInformation("Exercise not found!");
            }

            return RedirectToAction("Exercises");
        }

        // Helper method to validate AnswerChoices format
        private bool IsValidAnswerChoices(string answerChoices)
        {
            try
            {
                // Check if the input is valid JSON
                JsonDocument.Parse(answerChoices);
                return true;
            }
            catch (JsonException)
            {
                // If not JSON, check if it's comma-separated values
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
                    JsonDocument.Parse(answerChoices);
                    return answerChoices;
                }
                catch (JsonException)
                {
                    var values = answerChoices.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return JsonSerializer.Serialize(values); 
                }
            }
            return null; 
        }
        // POST: Admin/ArchiveExercise
        [HttpPost]
        public async Task<IActionResult> ArchiveExercise(int ExerciseID)
        {
            var exercise = _context.InteractiveExercises.Find(ExerciseID);
            if (exercise != null)
            {
                _context.InteractiveExercises.Remove(exercise);
                await _context.SaveChangesAsync();

                await _logService.LogEvent($"Archived exercise ID: {ExerciseID}");
            }
            return RedirectToAction("Exercises");
        }

        public IActionResult Lessons(string searchQuery, int page = 1, int pageSize = 5)
        {
            var lessonsQuery = _context.Lessons.Include(l => l.Language).AsQueryable();
            var languages = _context.Language.ToList();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                lessonsQuery = lessonsQuery.Where(l => l.Title.Contains(searchQuery)
                                                   || l.Language.LanguageName.Contains(searchQuery)
                                                   || l.LessonType.Contains(searchQuery));
            }

            int totalLessons = lessonsQuery.Count();
            var lessons = lessonsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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
                var lessonsQuery = _context.Lessons.Include(l => l.Language).AsQueryable();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    lessonsQuery = lessonsQuery.Where(l =>
                        l.Title.Contains(searchQuery) ||
                        l.LessonType.Contains(searchQuery) ||
                        l.DifficultyLevel.Contains(searchQuery) ||
                        (l.Language != null && l.Language.LanguageName.Contains(searchQuery))
                    );
                }

                int totalLessons = lessonsQuery.Count();
                var lessons = lessonsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
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
        public async Task<IActionResult> ArchiveLesson(int LessonID)
        {
            var lesson = _context.Lessons.Find(LessonID);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
               await _context.SaveChangesAsync();

                await _logService.LogEvent($"Archived lesson: {lesson.Title}");
            }
            return RedirectToAction("Lessons");
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



        public IActionResult Progress()
        {
            return View("~/Views/User/Admin/Progress.cshtml");
        }

        public async Task<IActionResult> Students(string search, int page = 1, int pageSize = 10)
        {
            // Fetch all users first
            var allUsers = await _userManager.Users.ToListAsync();

            // Filter users who are in the "Student" role
            var students = new List<IdentityUser>();
            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    students.Add(user);
                }
            }

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrEmpty(search))
            {
                students = students.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Pagination
            int totalStudents = students.Count;
            students = students.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalStudents / pageSize);

            return View("~/Views/User/Admin/Students.cshtml", students);
        }
        public async Task<IActionResult> FilterStudents(string search, int page = 1, int pageSize = 10)
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var students = new List<IdentityUser>();

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

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalStudents / pageSize);

            Console.WriteLine($"Pagination Debug: Total Students = {totalStudents}, Total Pages = {ViewBag.TotalPages}, Current Page = {ViewBag.CurrentPage}");

            return PartialView("~/Views/User/Admin/Partial/_StudentsTablePartial.cshtml", students);
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
                await _logService.LogEvent($"Updated student email: {Email}");
                return NotFound("Student not found.");
            }

            user.Email = Email;
            user.UserName = Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {

                return RedirectToAction("Students");
            }

            return BadRequest("Failed to update student.");
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
                await _logService.LogEvent($"Archived student ID: {Id}");
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
