using eSaysay.Data;
using eSaysay.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSaysay.Controllers
{
    public class UserProgressController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProgressController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CompleteLesson(int lessonId, string userId)
        {
            var progress = await _context.UserProgress
                .FirstOrDefaultAsync(up => up.LessonID == lessonId && up.UserID == userId);

            if (progress != null)
            {
                progress.CompletionStatus = "Completed";
                progress.LastAccessedDate = DateTime.UtcNow;
                progress.TimeSpent = 0;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
    }

}
