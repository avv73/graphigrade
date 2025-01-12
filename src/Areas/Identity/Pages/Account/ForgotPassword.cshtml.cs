// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using GraphiGrade.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using GraphiGrade.Configuration;
using GraphiGrade.Services.Identity.Abstractions;
using Microsoft.Extensions.Options;
using GraphiGrade.Extensions;

namespace GraphiGrade.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender; 
        private readonly RecaptchaConfig _recaptchaConfig;
        private readonly ICaptchaValidator _captchaValidator;

        public ForgotPasswordModel(
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
        /// Exposes the Recaptcha forgot password action name to the frontend.
        /// </summary>
        public string RecaptchaResetPasswordActionName => _recaptchaConfig.RecaptchaResetPasswordActionName;

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Validate reCaptcha.
                var captchaValidationResult = await _captchaValidator.ValidateCaptchaAsync(
                    Input.RecaptchaToken,
                    HttpContext.Request.GetUserAgent() ?? string.Empty,
                    HttpContext.Request.GetUserIp() ?? string.Empty,
                    _recaptchaConfig.RecaptchaResetPasswordActionName);

                if (captchaValidationResult != ValidationResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "A Recaptcha error occurred.");
                    return Page();
                }

                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
