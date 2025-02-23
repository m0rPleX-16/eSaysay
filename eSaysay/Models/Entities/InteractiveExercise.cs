using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eSaysay.Models.Entities
{
    public class InteractiveExercise
    {
        [Key]
        public int ExerciseID { get; set; }

        [ForeignKey("Lesson")]
        public int LessonID { get; set; }
        public Lesson Lesson { get; set; }

        [Required]
        [MaxLength(50)]
        public string ExerciseType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Content { get; set; }

        [Required]
        [MaxLength(100)]
        public string CorrectAnswer { get; set; }

        [MaxLength(20)]
        public string DifficultyLevel { get; set; }
    }
}