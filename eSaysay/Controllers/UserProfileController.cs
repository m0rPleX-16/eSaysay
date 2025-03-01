using eSaysay.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eSaysay.Models.ViewModels;
using eSaysay.Models.Entities;

namespace eSaysay.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Profile(string userId)
        {
            var userBadges = await _context.UserBadges
                .Where(ub => ub.UserID == userId)
                .Include(ub => ub.Badge)
                .ToListAsync();

            var allBadges = await _context.Badges.ToListAsync();

            var viewModel = new BadgeViewModel
            {
                EarnedBadges = userBadges.Select(ub => ub.Badge).ToList(),
                UnearnedBadges = allBadges.Where(b => !userBadges.Any(ub => ub.BadgeID == b.BadgeID)).ToList()
            };

            return View(viewModel);
        }
    }

}
