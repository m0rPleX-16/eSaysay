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
using System.ComponentModel.DataAnnotations;

namespace eSaysay.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly SecurityLogService _logService;

        public NotificationController(ILogger<DashboardController> logger,
            ApplicationDbContext context, SecurityLogService logService)
        {
            _logger = logger;
            _context = context;
            _logService = logService;
        }
            
        public async Task<IActionResult> GetUnreadNotifications(string userId)
        {
            _logger.LogInformation($"[GetUnreadNotifications] Fetching unread notifications for user {userId}.");

            var notifications = await _context.Notification
                .Where(n => n.UserID == userId && !n.IsRead)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            _logger.LogInformation($"[GetUnreadNotifications] Found {notifications.Count} unread notifications for user {userId}.");

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
