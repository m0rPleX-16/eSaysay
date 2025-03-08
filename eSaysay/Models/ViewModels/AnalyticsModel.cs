using eSaysay.Models.Entities;
using System.Collections.Generic;
namespace eSaysay.Models.ViewModels
{
    public class AnalyticsModel
    {
        public int TotalUsers { get; set; }
        public double? AvgScore { get; set; }
        public int? TotalTimeSpent { get; set; }
        public int TotalLessonsCompleted { get; set; }
        public List<string> AnalyticsDates { get; set; }
        public List<double> AnalyticsScores { get; set; }
        public List<int> AnalyticsTimeSpent { get; set; }
        public List<int> LessonsCompleted { get; set; }
    }

}
