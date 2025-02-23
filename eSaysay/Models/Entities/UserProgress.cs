using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class UserProgress
    {
        [Key]
        public int ProgressID { get; set; } // Primary Key

        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; } // Foreign Key referencing Users (Identity framework uses string GUID)
        public IdentityUser User { get; set; }

        [Required]
        [ForeignKey("Lesson")]
        public int LessonID { get; set; } // Foreign Key referencing Lesson
        public Lesson Lesson { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public string CompletionStatus { get; set; } = "Not Started"; 

        [Range(0, 100)]
        public double? Score { get; set; } // Nullable, as some lessons might not have scores

        [Range(0, int.MaxValue)]
        public int TimeSpent { get; set; } = 0; // Time spent in seconds

        [Required]
        public DateTime LastAccessedDate { get; set; } = DateTime.UtcNow; // Last accessed timestamp
    }
}
