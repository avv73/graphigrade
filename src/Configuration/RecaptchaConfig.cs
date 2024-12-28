using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configuration;

public class RecaptchaConfig : IValidatableObject
{
    [Required]
    public string SiteKey { get; set; } = null!;

    [Required]
    public string SecretKey { get; set; } = null!;

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
    }
}
