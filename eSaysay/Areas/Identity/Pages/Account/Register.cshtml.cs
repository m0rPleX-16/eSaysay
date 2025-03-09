    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    #nullable disable

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading;
    using System.Threading.Tasks;
    using eSaysay.Models.Entities;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using eSaysay.Data;
    using eSaysay.Models;
using System.Text.Json;

    namespace eSaysay.Areas.Identity.Pages.Account
    {
        public class RegisterModel : PageModel
        {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        //private const string hCaptchaSecretKey = "ES_888792c6dd5344dea7bd802f2b290bab";

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public string ReturnUrl { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public IList<AuthenticationScheme> ExternalLogins { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public class InputModel
            {
                /// <summary>
                ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
                ///     directly from your code. This API may change or be removed in future releases.
                /// </summary>

                [Required]
                [Display(Name = "First Name")]
                public string FirstName { get; set; }

                [Required]
                [Display(Name = "Last Name")]
                public string LastName { get; set; }

                [Required]
                [Display(Name = "Middle Name")]
                public string MiddleName { get; set; }

                [Required]
                [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
                public int Age { get; set; }

                [Required]
                [Display(Name = "Gender")]
                public string Gender { get; set; }

                [Required]
                [DataType(DataType.Date)]
                [Display(Name = "Birthday")]
                public DateTime Birthday { get; set; }

                [Required]
                [EmailAddress]
                [Display(Name = "Email")]
                public string Email { get; set; }

                /// <summary>
                ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
                ///     directly from your code. This API may change or be removed in future releases.
                /// </summary>
                [Required]
                [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
                [DataType(DataType.Password)]
                [Display(Name = "Password")]
                public string Password { get; set; }

                /// <summary>
                ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
                ///     directly from your code. This API may change or be removed in future releases.
                /// </summary>
                [DataType(DataType.Password)]
                [Display(Name = "Confirm password")]
                [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
                public string ConfirmPassword { get; set; }

                [Required]
                [Display(Name = "CAPTCHA Code")]
                public string CaptchaCode { get; set; }

            //[Required]
            //[Display(Name = "hCaptcha Token")]
            //public string RecaptchaToken { get; set; }

            //[Required]
            //[Display(Name = "hCaptcha Response")]
            //public string hCaptchaResponse { get; set; }
        }

            public async Task OnGetAsync(string returnUrl = null)
            {
                ReturnUrl = returnUrl;
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            }

            public async Task<IActionResult> OnPostAsync(string returnUrl = null)
            {
                returnUrl ??= Url.Content("~/");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var storedCaptcha = HttpContext.Session.GetString("CaptchaCode");

            if (!string.Equals(Input.CaptchaCode, storedCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"CAPTCHA validation failed. Input: {Input.CaptchaCode}, Stored: {storedCaptcha}");
                ModelState.AddModelError(string.Empty, "CAPTCHA verification failed.");
                return Page();
            }

            if (ModelState.IsValid)
                {
                    var user = CreateUser();

                    user.FirstName = Input.FirstName;
                    user.LastName = Input.LastName;
                    user.MiddleName = Input.MiddleName;
                    user.Age = Input.Age;
                    user.Gender = Input.Gender;
                    user.Birthday = Input.Birthday;
                    user.RegistrationDate = DateTime.UtcNow;

                    await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        await _userManager.AddToRoleAsync(user, "Student");
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                        // Log Registration Event
                        var log = new SecurityLog
                        {
                            UserID = user.Id,
                            Event = "User Registered",
                            Timestamp = DateTime.UtcNow,
                            IPAddress = ipAddress
                        };

                        _context.SecurityLog.Add(log);
                        await _context.SaveChangesAsync();

                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);

                            // Check if the user has already selected their experience level
                            var registeredUser = await _userManager.FindByIdAsync(userId);

                            if (registeredUser != null && string.IsNullOrEmpty(registeredUser.LanguageExperience))
                            {
                                return RedirectToAction("SelectLanguageExperience", "Dashboard"); // No experience level set, redirect to selection
                            }

                            return RedirectToAction("StudentDashboard", "Dashboard"); // Experience level already set, go to dashboard
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                // If we got this far, something failed, redisplay form
                return Page();
            }
        private ApplicationUser CreateUser()
            {
                try
                {
                    return Activator.CreateInstance<ApplicationUser>();
                }
                catch
                {
                    throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                        $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                        $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
                }
            }

            private IUserEmailStore<ApplicationUser> GetEmailStore()
            {
                if (!_userManager.SupportsUserEmail)
                {
                    throw new NotSupportedException("The default UI requires a user store with email support.");
                }
                return (IUserEmailStore<ApplicationUser>)_userStore;
            }
        }
    }
