using System.Collections.Generic;
using eSaysay.Models.Entities;  

namespace eSaysay.Models.ViewModels
{
    public class ProgressViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int InProgressLessons { get; set; }
        public int NotStartedLessons { get; set; }
        public double AverageScore { get; set; }
        public List<ProgressDetailViewModel> ProgressData { get; set; }
    }

    public class ProgressDetailViewModel
    {
        public string UserName { get; set; }
        public string LessonName { get; set; }
        public string CompletionStatus { get; set; }
        public double Score { get; set; }
        public int TimeSpent { get; set; }
        public DateTime LastAccessedDate { get; set; }
    }
}
