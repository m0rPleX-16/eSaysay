using eSaysay.Models.Entities;

namespace eSaysay.Models.ViewModels
{
    public class ExerciseResultViewModel
    {
        public InteractiveExercise Exercise { get; set; }
        public bool IsCorrect { get; set; }
    }
}
