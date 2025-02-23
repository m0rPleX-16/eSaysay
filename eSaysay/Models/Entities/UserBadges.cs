using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class UserBadge
    {
        [Key]
        public int UserBadgeID { get; set; } // Primary Key

        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; } // Foreign Key to User (Identity framework uses string GUID)
        public IdentityUser User { get; set; }

        [Required]
        [ForeignKey("Badge")]
        public int BadgeID { get; set; } // Foreign Key to Badge
        public Badge Badge { get; set; }

        [Required]
        public DateTime DateEarned { get; set; } = DateTime.UtcNow; // Stores when the user earned the badge
    }
}

