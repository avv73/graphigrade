// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GraphiGrade.Configuration;
using Microsoft.AspNetCore.Authorization;
using GraphiGrade.Models.Identity;
using GraphiGrade.Services.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using GraphiGrade.Extensions;

namespace GraphiGrade.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RecaptchaConfig _recaptchaConfig;
        private readonly ICaptchaValidator _captchaValidator;

        public ResendEmailConfirmationModel(
            UserManager<User> userManager, 
            IEmailSender emailSender,
            IOptions<GraphiGradeConfig> configuration,
            ICaptchaValidator captchaValidator)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _recaptchaConfig = configuration.Value.RecaptchaConfig;
            _captchaValidator = captchaValidator;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Exposes the Recaptcha site key to the frontend.
        /// </summary>
        public string RecaptchaSiteKey => _recaptchaConfig.SiteKey;

        /// <summary>
        /// Exposes the Recaptcha resend email confirmation action name to the frontend.
        /// </summary>
        public string RecaptchaResendEmailConfirmation => _recaptchaConfig.RecaptchaResendEmailConfirmationActionName;

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
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            /// Token to validate reCaptcha against.
            /// </summary>
            [Required]
            public string RecaptchaToken { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate reCaptcha.
            var captchaValidationResult = await _captchaValidator.ValidateCaptchaAsync(
                Input.RecaptchaToken,
                HttpContext.Request.GetUserAgent() ?? string.Empty,
                HttpContext.Request.GetUserIp() ?? string.Empty,
                _recaptchaConfig.RecaptchaResendEmailConfirmationActionName);

            if (captchaValidationResult != ValidationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "A Recaptcha error occurred.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "If an account with this address has been registered, you will receive a verification email in your inbox.");
                return Page();
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Your email is already confirmed.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "If an account with this address has been registered, you will receive a verification email in your inbox.");
            return Page();
        }
    }
}
