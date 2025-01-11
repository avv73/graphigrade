using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configuration;

public class RecaptchaConfig : IValidatableObject
{
    [Required]
    public string SiteKey { get; set; } = null!;

    [Required]
    public string SecretKey { get; set; } = null!;

    [Required]
    public string GoogleApiKey { get; set; } = null!;

    [Required]
    public string RecaptchaAssessUrl { get; set; } = null!;

    [Required] 
    public string RecaptchaRegisterActionName { get; set; } = null!;

    [Required]
    public string RecaptchaLoginActionName { get; set; } = null!;

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
            yield return new ValidationResult($"'{nameof(RecaptchaAssessUrl)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(RecaptchaLoginActionName))
        {
            yield return new ValidationResult($"'{nameof(RecaptchaAssessUrl)}' is missing from configuration!");
        }
    }
}