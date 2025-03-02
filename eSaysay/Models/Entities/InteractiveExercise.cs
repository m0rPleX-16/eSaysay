using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eSaysay.Models.Entities
{
    public class InteractiveExercise
    {
        [Key]
        public int ExerciseID { get; set; }

        [ForeignKey("LessonID")]
        public int LessonID { get; set; }
        [ValidateNever]
        public Lesson Lesson { get; set; }

        [Required]
        [MaxLength(50)]
        public string ExerciseType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Content { get; set; }

        [MaxLength(200)]
        public string? ContentTranslate { get; set; }

        [Required]
        [MaxLength(100)]
        public string CorrectAnswer { get; set; }

        [MaxLength(300)]
        public string? AnswerChoices { get; set; }

        [MaxLength(500)]
        public string? Hint { get; set; } 

        [MaxLength(20)]
        public string DifficultyLevel { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
