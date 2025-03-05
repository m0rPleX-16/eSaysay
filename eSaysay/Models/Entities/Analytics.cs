using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class Analytics
    {
        [Key]
        public int AnalyticsID { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey("Lesson")]
        public int LessonCompleted { get; set; }
        public Lesson Lesson { get; set; }

        [Required]
        [Range(0, 100)]
        public double AverageScore { get; set; }

        [Required]
        [Range(0, 1440)]
        public int TimeSpent { get; set; }
    }
}
