using GraphiGrade.Configurations;

namespace GraphiGrade.Extensions.DependencyInjection;

public static class RegisterGraphiGradeConfigExtensions
{
    public static GraphiGradeConfig AddGraphiGradeConfig(this WebApplicationBuilder builder)
    {
        var configBuilder = builder.Services.AddOptions<GraphiGradeConfig>()
            .Bind(builder.Configuration.GetSection("GraphiGradeConfig"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        GraphiGradeConfig config = new()
        {
            DbConnectionString = null,
            JwtSecretKey = null,
            JwtExpirationInSeconds = 0,
            JwtIssuer = null,
            JwtAudience = null
        };

        builder.Configuration.GetSection("GraphiGradeConfig").Bind(config);

        return config;
    }
}
