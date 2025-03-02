using eSaysay.Models.Entities;

namespace eSaysay.Models.ViewModels
{
    public class BadgeViewModel
    {
        public List<Badge> EarnedBadges { get; set; }
        public List<Badge> UnearnedBadges { get; set; }
    }
}
