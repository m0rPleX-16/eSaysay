using eSaysay.Data;
using eSaysay.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSaysay.Controllers
{
    public class UserProgressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BadgeService _badgeService;

        public UserProgressController(ApplicationDbContext context, BadgeService badgeService)
        {
            _context = context;
            _badgeService = badgeService;
        }

        public async Task<IActionResult> CompleteLesson(int lessonId, string userId)
        {
            var progress = await _context.UserProgress
                .FirstOrDefaultAsync(up => up.LessonID == lessonId && up.UserID == userId);

            if (progress != null)
            {
                progress.CompletionStatus = "Completed";
                progress.LastAccessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Check if user has earned new badges
                await _badgeService.CheckAndAwardBadgesAsync(userId);
            }

            return RedirectToAction("Dashboard");
        }
    }

}
