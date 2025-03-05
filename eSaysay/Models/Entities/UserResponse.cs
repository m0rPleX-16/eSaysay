using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class UserResponse
    {
        [Key]
        public int ResponseID { get; set; } 
        [ForeignKey("User")]
        public string UserID { get; set; } // Foreign Key referencing Users (Identity framework uses string GUID)
        public ApplicationUser User { get; set; }

        [Required]
        [ForeignKey("InteractiveExercise")]
        public int ExerciseID { get; set; } // Foreign Key referencing InteractiveExercise
        public InteractiveExercise Exercise { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(MAX)")] // Stores user's answer
        public string UserAnswer { get; set; }

        [Required]
        public bool IsCorrect { get; set; } // True if the answer is correct, False otherwise

        [Required]
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow; // Stores the timestamp of the attempt
    }
}
