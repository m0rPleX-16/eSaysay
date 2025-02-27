using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using eSaysay.Data;
using eSaysay.Models.Entities;
using System.Security.Claims;

namespace eSaysay.Services
{
    public class SecurityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogEvent(string eventType)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (userId != null)
            {
                var log = new SecurityLog
                {
                    UserID = userId,
                    Event = eventType,
                    Timestamp = DateTime.UtcNow,
                    IPAddress = ipAddress
                };

                _context.SecurityLog.Add(log);
                await _context.SaveChangesAsync();
            }
        }
    }
}
