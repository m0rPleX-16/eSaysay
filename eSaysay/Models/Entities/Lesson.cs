using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class Lesson
    {
        [Key]
        public int LessonID { get; set; }
        
        [Required]
        public int LanguageID { get; set; }

        [ForeignKey("LanguageID")]
        public Language Language { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(150)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string LessonType { get; set; }

        [Required]
        [MaxLength(20)]
        public string DifficultyLevel { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
