using Microsoft.AspNetCore.Identity;
using System;
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public DateTime Birthday { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}
