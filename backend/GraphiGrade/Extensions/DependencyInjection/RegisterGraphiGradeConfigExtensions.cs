using GraphiGrade.Configurations;

namespace GraphiGrade.Extensions.DependencyInjection;

public static class RegisterGraphiGradeConfigExtensions
{
    public static void AddGraphiGradeConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<GraphiGradeConfig>()
            .Bind(builder.Configuration.GetSection("GraphiGradeConfig"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
