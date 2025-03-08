using System;
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
        public int TotalPendingLessons => TotalLessons - (CompletedLessons + InProgressLessons); 
        public double AverageScore { get; set; }
        public List<ProgressDetailViewModel> ProgressData { get; set; } = new List<ProgressDetailViewModel>(); 
    }

    public class ProgressDetailViewModel
    {
        public string UserName { get; set; } = "Student"; 
        public string LessonName { get; set; } = "Unknown Lesson"; 
        public string CompletionStatus { get; set; } = "Not Started"; 
        public double Score { get; set; } = 0; 
        public int? TimeSpent { get; set; } = 0; 
        public DateTime? LastAccessedDate { get; set; } = null; 
    }
}
