using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configuration;

public class RecaptchaConfig : IValidatableObject
{
    public string SiteKey { get; set; }

    public string SecretKey { get; set; }

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
