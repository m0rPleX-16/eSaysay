using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models {
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public int Age { get; set; } = 0;
        public string Gender { get; set; } = string.Empty;
        public DateTime Birthday { get; set; } = DateTime.Now;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public bool IsArchived { get; set; } = false;
    }
}