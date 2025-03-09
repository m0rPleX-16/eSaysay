using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class SecurityLog
    {
        [Key]
        public int LogID { get; set; }

        [ForeignKey("User")]
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Event { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(45)]
        public string IPAddress { get; set; }
    }
}