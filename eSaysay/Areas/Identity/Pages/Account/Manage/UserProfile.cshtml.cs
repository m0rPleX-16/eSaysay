using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using eSaysay.Models;
using eSaysay.Services; // Import the EncryptionService

namespace eSaysay.Areas.Identity.Pages.Account.Manage
{
    public class UserProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly EncryptionService _encryptionService; // Inject Encryption Service

        public UserProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            EncryptionService encryptionService) // Injected EncryptionService
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _encryptionService = encryptionService;
        }

        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Middle Name")]
            public string MiddleName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [Required]
            [Range(1, 120, ErrorMessage = "Please enter a valid age.")]
            [Display(Name = "Age")]
            public int Age { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Birthday")]
            public DateTime Birthday { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            // **🔹 Decrypt sensitive user details before displaying**
            Input = new InputModel
            {
                NewEmail = email,
                FirstName = _encryptionService.DecryptData(user.FirstName),
                MiddleName = _encryptionService.DecryptData(user.MiddleName),
                LastName = _encryptionService.DecryptData(user.LastName),
                Gender = _encryptionService.DecryptData(user.Gender),
                Age = user.Age,
                Birthday = user.Birthday
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // **🔹 Encrypt sensitive user details before storing**
            user.FirstName = _encryptionService.EncryptData(Input.FirstName);
            user.MiddleName = _encryptionService.EncryptData(Input.MiddleName);
            user.LastName = _encryptionService.EncryptData(Input.LastName);
            user.Gender = _encryptionService.EncryptData(Input.Gender);
            user.Age = Input.Age;
            user.Birthday = Input.Birthday;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated.";
            return RedirectToPage();
        }
    }
}
