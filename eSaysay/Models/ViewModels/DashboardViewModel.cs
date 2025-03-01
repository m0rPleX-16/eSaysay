using eSaysay.Models.Entities;

namespace eSaysay.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Lesson> Lessons { get; set; }
        public List<Notification> Notifications { get; set; }
    }
}
