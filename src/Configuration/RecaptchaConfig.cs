using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configuration;

public class RecaptchaConfig : IValidatableObject
{
    public string SiteKey { get; set; } = null!;

    public string SecretKey { get; set; } = null!;

    public string GoogleApiKey { get; set; } = null!;

    public string RecaptchaAssessUrl { get; set; } = null!;

    public string RecaptchaRegisterActionName { get; set; } = null!;

    public string RecaptchaLoginActionName { get; set; } = null!;

    public string RecaptchaResendEmailConfirmationActionName { get; set; } = null!;

    public string RecaptchaResetPasswordActionName { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(SiteKey))
        {
            yield return new ValidationResult($"'{nameof(SiteKey)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            yield return new ValidationResult($"'{nameof(SecretKey)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(GoogleApiKey))
        {
            yield return new ValidationResult($"'{nameof(GoogleApiKey)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(RecaptchaAssessUrl))
        {
            yield return new ValidationResult($"'{nameof(RecaptchaAssessUrl)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(RecaptchaRegisterActionName))
        {
            yield return new ValidationResult($"'{nameof(RecaptchaRegisterActionName)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(RecaptchaLoginActionName))
        {
            yield return new ValidationResult($"'{nameof(RecaptchaLoginActionName)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(RecaptchaResendEmailConfirmationActionName))
        {
            yield return new ValidationResult($"'{nameof(RecaptchaResendEmailConfirmationActionName)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(RecaptchaResetPasswordActionName))
        {
            yield return new ValidationResult($"'{nameof(RecaptchaResetPasswordActionName)}' is missing from configuration!");
        }
    }
}