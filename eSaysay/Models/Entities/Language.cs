using System.ComponentModel.DataAnnotations;

namespace eSaysay.Models.Entities
{
    public class Language
    {
        [Key]
        public int LanguageID { get; set; }

        [Required]
        [MaxLength(50)]
        public string LanguageName { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
