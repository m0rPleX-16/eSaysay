using System;
using System.ComponentModel.DataAnnotations;

namespace eSaysay.Models.Entities
{
    public class Badge
    {
        [Key]
        public int BadgeID { get; set; } // Primary Key

        [Required]
        [MaxLength(100)] // Limits name length
        public string BadgeName { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [StringLength(255)]
        public string Criteria { get; set; }
    }
}
