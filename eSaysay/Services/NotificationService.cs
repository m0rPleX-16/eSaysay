using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSaysay.Data;
using eSaysay.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eSaysay.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, bool isRead = false)
        {
            try
            {
                var notification = new Notification
                {
                    UserID = userId,
                    Title = title,
                    Message = message,
                    IsRead = isRead,
                    DateCreated = DateTime.UtcNow
                };

                _context.Notification.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification: {ex.Message}");
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notification
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notification.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notification.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notification.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
