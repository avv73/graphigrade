using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Configurations;

public class GraphiGradeConfig : IValidatableObject
{
    public required string DbConnectionString { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(DbConnectionString))
        {
            yield return new ValidationResult($"'{nameof(DbConnectionString)}' is missing from configuration!");
        }
    }
}
