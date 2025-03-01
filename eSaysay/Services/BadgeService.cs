using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSaysay.Data;
using eSaysay.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using eSaysay.Services;

namespace eSaysay.Services
{
    public class BadgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly ILogger<BadgeService> _logger;
        private readonly SecurityLogService _logService;

        public BadgeService(ApplicationDbContext context, NotificationService notificationService, ILogger<BadgeService> logger, SecurityLogService logService)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _logService = logService;
        }

        public async Task CheckAndAwardBadgesAsync(string userId)
        {
            try
            {
                var userProgress = await _context.UserProgress
                    .Where(up => up.UserID == userId && up.CompletionStatus == "Completed")
                    .ToListAsync();

                var analytics = await _context.Analytics
                    .Where(a => a.UserID == userId)
                    .ToListAsync();

                var earnedBadges = new List<int>();

                // Example badge criteria checks
                if (userProgress.Count >= 3)
                    earnedBadges.Add(await GetBadgeIdByName("Beginner"));

                if (userProgress.Count >= 10)
                    earnedBadges.Add(await GetBadgeIdByName("Dedicated Learner"));

                if (analytics.Any(a => a.TimeSpent <= 5))
                    earnedBadges.Add(await GetBadgeIdByName("Speed Learner"));

                if (analytics.Any(a => a.AverageScore == 100))
                    earnedBadges.Add(await GetBadgeIdByName("Perfectionist"));

                if (analytics.Sum(a => a.TimeSpent) >= 100)
                    earnedBadges.Add(await GetBadgeIdByName("Time Master"));

                // Save new badges and send notifications
                foreach (var badgeId in earnedBadges)
                {
                    var exists = await _context.UserBadges
                        .AnyAsync(ub => ub.UserID == userId && ub.BadgeID == badgeId);

                    if (!exists)
                    {
                        var badge = await _context.Badges.FindAsync(badgeId);
                        if (badge != null)
                        {
                            _context.UserBadges.Add(new UserBadge
                            {
                                UserID = userId,
                                BadgeID = badgeId,
                                DateEarned = DateTime.UtcNow
                            });

                            // Send in-app notification
                            await _notificationService.SendNotificationAsync(
                                userId,
                                "🎉 New Badge Earned!",
                                $"Congratulations! You earned the '{badge.BadgeName}' badge!",
                                isRead: false
                            );
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in awarding badges: {ex.Message}");
            }
        }

        // Retrieve all badges
        public async Task<List<Badge>> GetAllBadgesAsync()
        {
            return await _context.Badges.ToListAsync();
        }

        // Add a new badge
        public async Task AddBadgeAsync(Badge badge)
        {
            try
            {
                _context.Badges.Add(badge);
                await _context.SaveChangesAsync();
                await _logService.LogEvent($"Created new badge: {badge.BadgeName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding badge: {ex.Message}");
            }
        }

        // Update an existing badge
        public async Task UpdateBadgeAsync(Badge badge)
        {
            try
            {
                _context.Badges.Update(badge);
                await _context.SaveChangesAsync();
                await _logService.LogEvent($"Edited new badge: {badge.BadgeName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating badge: {ex.Message}");
            }
        }

        // Archive (soft delete) a badge
        public async Task ArchiveBadgeAsync(int badgeId)
        {
            try
            {
                var badge = await _context.Badges.FindAsync(badgeId);
                if (badge != null)
                {
                    _context.Badges.Remove(badge);
                    await _context.SaveChangesAsync();
                    await _logService.LogEvent($"Archived badge: {badge.BadgeName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error archiving badge: {ex.Message}");
            }
        }

        private async Task<int> GetBadgeIdByName(string badgeName)
        {
            var badge = await _context.Badges.FirstOrDefaultAsync(b => b.BadgeName == badgeName);
            return badge?.BadgeID ?? 0;
        }
    }
}
