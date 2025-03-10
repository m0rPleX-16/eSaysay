using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using eSaysay.Models;

namespace eSaysay.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool ArchiveInsteadOfDelete { get; set; }
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (string.IsNullOrEmpty(Password) || !await _userManager.CheckPasswordAsync(user, Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            if (ArchiveInsteadOfDelete)
            {
                // Archive the user instead of deleting
                user.IsArchived = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Error archiving user with ID '{UserId}'.", user.Id);
                    ModelState.AddModelError(string.Empty, "Failed to archive the account.");
                    return Page();
                }

                _logger.LogInformation("User with ID '{UserId}' archived their account.", user.Id);
            }
            else
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError("Error deleting user with ID '{UserId}'.", userId);
                    ModelState.AddModelError(string.Empty, "Failed to delete the account.");
                    return Page();
                }

                _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);
            }

            // **Ensure the user is logged out before redirecting**
            await _signInManager.SignOutAsync();

            // **Redirect to the Home Index**
            return RedirectToAction("Index", "Home");
        }
    }
}
