using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configuration;

public class MailgunConfig : IValidatableObject
{
    public string MailgunApiKey { get; set; }

    public string MailgunEndpointUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(MailgunApiKey))
        {
            yield return new ValidationResult($"'{nameof(MailgunApiKey)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(MailgunEndpointUrl))
        {
            yield return new ValidationResult($"'{nameof(MailgunEndpointUrl)}' is missing from configuration!");
        }
    }
}
