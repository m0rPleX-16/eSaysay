using eSaysay.Models.Entities;

namespace eSaysay.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Lesson> BeginnerLessons { get; set; } = new();
        public List<Lesson> IntermediateLessons { get; set; } = new();
        public List<Lesson> AdvancedLessons { get; set; } = new();
    }
}
