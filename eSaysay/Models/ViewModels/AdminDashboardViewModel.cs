namespace eSaysay.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public double AverageScore { get; set; }
        public int TotalTimeSpent { get; set; }
        public List<RecentActivityModel> RecentActivity { get; set; }

        public string FormattedAverageScore => AverageScore.ToString("0.00") + "%";
    }

    public class RecentActivityModel
    {
        public string FullName { get; set; }
        public string LessonTitle { get; set; }
        public string CompletionStatus { get; set; }
        public DateTime LastAccessedDate { get; set; }
    }
}
