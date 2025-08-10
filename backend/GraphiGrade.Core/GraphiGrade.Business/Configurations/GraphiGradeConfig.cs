using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Business.Configurations;

public class GraphiGradeConfig : IValidatableObject
{
    public required string DbConnectionString { get; set; }

    public required string JwtSecretKey { get; set; }

    public required int JwtExpirationInSeconds { get; set; }

    public required string JwtIssuer { get; set; }

    public required string JwtAudience { get; set; }

    public required int MaximumBytesSizeOfResultPattern { get; set; }

    public long MaximumBase64CharsInResultPattern => (long)Math.Ceiling(MaximumBytesSizeOfResultPattern / 3.0) * 4;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(DbConnectionString))
        {
            yield return new ValidationResult($"'{nameof(DbConnectionString)}' is missing from configuration!", new[] { nameof(DbConnectionString) });
        }

        if (string.IsNullOrWhiteSpace(JwtSecretKey))
        {
            yield return new ValidationResult($"'{nameof(JwtSecretKey)}' is missing from configuration!", new[] { nameof(JwtSecretKey) });
        }

        if (JwtExpirationInSeconds <= 0)
        {
            yield return new ValidationResult($"'{nameof(JwtExpirationInSeconds)}' is incorrectly configured!", new[] { nameof(JwtExpirationInSeconds) });
        }

        if (string.IsNullOrWhiteSpace(JwtIssuer))
        {
            yield return new ValidationResult($"'{nameof(JwtIssuer)}' is missing from configuration!", new[] { nameof(JwtIssuer) });
        }

        if (string.IsNullOrWhiteSpace(JwtAudience))
        {
            yield return new ValidationResult($"'{nameof(JwtAudience)}' is missing from configuration!", new[] { nameof(JwtAudience) });
        }

        if (MaximumBytesSizeOfResultPattern <= 0)
        {
            yield return new ValidationResult($"'{nameof(MaximumBytesSizeOfResultPattern)}' is incorrectly configured!", new[] { nameof(MaximumBytesSizeOfResultPattern) });
        }
    }
}
