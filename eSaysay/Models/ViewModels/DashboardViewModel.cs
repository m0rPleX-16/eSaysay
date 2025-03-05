using eSaysay.Models.Entities;

namespace eSaysay.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Lesson> Lessons { get; set; }
        public List<Notification> Notifications { get; set; }
        public Analytics? Analytics { get; set; }
        public AdaptiveLearning AdaptiveLearning { get; set; }
        public List<UserProgress> UserProgress { get; set; }
    }
}
