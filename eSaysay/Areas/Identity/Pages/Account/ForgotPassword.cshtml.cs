using eSaysay.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var users = await _userManager.Users
            .Where(u => u.Email == Input.Email)
            .ToListAsync();

        if (users.Count == 0 || !users.Any(u => _userManager.IsEmailConfirmedAsync(u).Result))
        {
            // Don't reveal that the user does not exist or is not confirmed
            _logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", Input.Email);
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        if (users.Count > 1)
        {
            _logger.LogError("Multiple users found with the same email: {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "An error occurred. Please contact support.");
            return Page();
        }

        var user = users.First();

        try
        {
            // Generate password reset token
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Create callback URL
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            // Send email
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Reset Your Password - eSaysay",
                $@"
    <html>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                <h2 style='color: #007BFF;'>Reset Your Password</h2>
                <p>Hello,</p>
                <p>We received a request to reset your password for your eSaysay account. If you did not make this request, you can safely ignore this email.</p>
                <p>To reset your password, click the button below:</p>
                <p style='text-align: center; margin: 30px 0;'>
                    <a href='{callbackUrl}' 
                       style='background-color: #007BFF; color: #fff; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-size: 16px;'>
                        Reset Password
                    </a>
                </p>
                <p>If the button above doesn't work, copy and paste the following link into your browser:</p>
                <p style='word-break: break-all; color: #007BFF;'>{callbackUrl}</p>
                <p>This link will expire in 1 hour for security reasons.</p>
                <p>If you have any questions, feel free to contact our support team at <a href='mailto:suppsaysay119.com' style='color: #007BFF;'>support@esaysay.com</a>.</p>
                <p>Thank you,<br>The eSaysay Team</p>
            </div>
        </body>
    </html>
    ");
            _logger.LogInformation("Password reset email sent to {Email}", Input.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
            return Page();
        }

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}