using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GraphiGrade.ValidatorAttributes.SchemaFilters;

public class PasswordValidationDescriptionFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var customAttributes = context.MemberInfo?.CustomAttributes;
        if (customAttributes != null)
        {
            var attr = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(PasswordValidationAttribute));

            if (attr != null)
            {
                schema.Extensions.Add("password", new OpenApiString("At least 6 characters, at least 1 of all of the following: uppercase, lowercase and special symbol"));
            }
        }
    }
}
