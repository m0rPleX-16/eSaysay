using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace eSaysay.Models.Entities
{
    public class Lesson
    {
        [Key]
        public int LessonID { get; set; }

        public int LanguageID { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(150)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string LessonType { get; set; }

        [MaxLength(20)]
        public string DifficultyLevel { get; set; }

        public string CreatedBy { get; set; }
        public IdentityUser CreatedByUser { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
