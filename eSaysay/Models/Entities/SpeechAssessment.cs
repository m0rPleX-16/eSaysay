using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class SpeechAssessment
    {
        [Key]
        public int AssessmentID { get; set; } // Primary Key

        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; } // Foreign Key to User (Identity framework uses string GUID)
        public IdentityUser User { get; set; }

        [Required]
        [ForeignKey("InteractiveExercise")]
        public int ExerciseID { get; set; } // Foreign Key to InteractiveExercise
        public InteractiveExercise Exercise { get; set; }

        [Required]
        [StringLength(255)]
        public string UserRecording { get; set; } // File path or URL of the user's voice recording

        [Required]
        [Range(0, 100)] 
        public double AccuracyScore { get; set; } // AI-based pronunciation accuracy score

        [StringLength(500)] 
        public string Feedback { get; set; } // AI-generated feedback for improvement

        [Required]
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow; // Stores timestamp of the assessment attempt
    }
}
