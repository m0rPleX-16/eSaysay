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
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(255)]
        public string UserRecording { get; set; } // File path or URL of the user's voice recording

        [Required]
        [Range(0, 100)] 
        public double AccuracyScore { get; set; } // AI-based pronunciation accuracy score

        [StringLength(500)] 
        public string Feedback { get; set; } // AI-generated feedback for improvement

        [Required]
        [Range(0, 100)]
        public double FluencyScore { get; set; } // Measures smoothness, speed, and pauses  

        [Required]
        [Range(0, 100)]
        public double OverallScore { get; set; } // A weighted score considering fluency & accuracy  

        [Range(0, 300)] // Allow up to 5 minutes (300 sec)
        public double SpeechDuration { get; set; } // Captures how long the user spoke  

        [Range(0, 500)]
        public int WordCount { get; set; } // Number of words spoken  

        [Required]
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow; // Stores timestamp of the assessment attempt
    }
}
