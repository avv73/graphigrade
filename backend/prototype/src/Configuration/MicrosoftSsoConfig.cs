using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configuration;

public class MicrosoftSsoConfig : IValidatableObject
{
    [Required]
    public bool IsEnabled { get; set; }

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;

    public string AuthorizationEndpoint { get; set; } = null!;

    public string TokenEndpoint { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsEnabled)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            yield return new ValidationResult($"'{nameof(ClientId)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            yield return new ValidationResult($"'{nameof(ClientSecret)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(AuthorizationEndpoint))
        {
            yield return new ValidationResult($"'{nameof(AuthorizationEndpoint)}' is missing from configuration!");
        }

        if (string.IsNullOrWhiteSpace(TokenEndpoint))
        {
            yield return new ValidationResult($"'{nameof(TokenEndpoint)}' is missing from configuration!");
        }
    }
}
