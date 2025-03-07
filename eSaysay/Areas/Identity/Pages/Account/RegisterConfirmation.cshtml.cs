// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using eSaysay.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace eSaysay.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public string Email { get; set; }
        public bool DisplayConfirmAccountLink { get; set; }
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;

            // Generate the email confirmation link
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);

            // Send the confirmation email
            await SendConfirmationEmailAsync(email, EmailConfirmationUrl);

            // For demonstration purposes, we'll display the link on the page
            DisplayConfirmAccountLink = true;

            return Page();
        }

        private async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            var smtpServer = _configuration["SmtpSettings:Server"];
            var smtpPort = _configuration["SmtpSettings:Port"];
            var smtpUsername = _configuration["SmtpSettings:Username"];
            var smtpPassword = _configuration["SmtpSettings:Password"];

            if (string.IsNullOrEmpty(smtpServer)) throw new ArgumentNullException(nameof(smtpServer), "SmtpServer is not configured.");
            if (string.IsNullOrEmpty(smtpPort)) throw new ArgumentNullException(nameof(smtpPort), "SmtpPort is not configured.");
            if (string.IsNullOrEmpty(smtpUsername)) throw new ArgumentNullException(nameof(smtpUsername), "SmtpUsername is not configured.");
            if (string.IsNullOrEmpty(smtpPassword)) throw new ArgumentNullException(nameof(smtpPassword), "SmtpPassword is not configured.");

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("eSaysay Support", smtpUsername));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Confirm your email";
            emailMessage.Body = new TextPart("html")
            {
                Text = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>."
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, int.Parse(smtpPort), true);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
