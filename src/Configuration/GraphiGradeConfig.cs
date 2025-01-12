using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configuration;

public class GraphiGradeConfig : IValidatableObject
{
    [Required]
    public string DbConnectionString { get; set; } = null!;

    [Required]
    public MailgunConfig MailgunConfig { get; set; } = null!;

    [Required]
    public RecaptchaConfig RecaptchaConfig { get; set; } = null!;
    
    [Required]
    public MicrosoftSsoConfig MicrosoftSsoConfig { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(DbConnectionString))
        {
            yield return new ValidationResult($"'{nameof(DbConnectionString)}' is missing from configuration!");
        }

        foreach (var validationResult in MailgunConfig.Validate(validationContext))
        {
            yield return validationResult;
        }

        foreach (var validationResult in RecaptchaConfig.Validate(validationContext))
        {
            yield return validationResult;
        }

        foreach (var validationResult in MicrosoftSsoConfig.Validate(validationContext))
        {
            yield return validationResult;
        }
    }
}