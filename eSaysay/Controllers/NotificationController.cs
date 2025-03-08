using System.Linq;
using System.Threading.Tasks;
using eSaysay.Data;
using eSaysay.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSaysay.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetUnreadNotifications(string userId)
        {
            var notifications = await _context.Notification
                .Where(n => n.UserID == userId && !n.IsRead)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            return Json(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _context.Notification.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(string userId, string message)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(message))
            {
                return BadRequest("Invalid notification data.");
            }

            var notification = new Notification
            {
                UserID = userId,
                Message = message,
                IsRead = false,
                DateCreated = DateTime.UtcNow
            };

            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
