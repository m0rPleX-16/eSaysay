using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; } // Primary Key

        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; } // Foreign Key referencing Users
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } // Notification Title

        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Message { get; set; } // Notification Message

        [Required]
        public bool IsRead { get; set; } = false; // Track if the user has seen it

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow; // Timestamp
    }
}
