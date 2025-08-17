using GraphiGrade.Business.Configurations;

namespace GraphiGrade.Web.Extensions.DependencyInjection;

public static class RegisterGraphiGradeConfigExtensions
{
    public static GraphiGradeConfig AddGraphiGradeConfig(this WebApplicationBuilder builder)
    {
        var configBuilder = builder.Services.AddOptions<GraphiGradeConfig>()
            .Bind(builder.Configuration.GetSection(nameof(GraphiGradeConfig)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        GraphiGradeConfig config = new()
        {
            DbConnectionString = null,
            JwtSecretKey = null,
            JwtExpirationInSeconds = 0,
            JwtIssuer = null,
            JwtAudience = null,
            MaximumBytesSizeOfResultPattern = 0,
            AzureBlobStorageConnectionString = null,
            AzureBlobStorageContainerName = null,
            JudgeServiceUrl = null
        };

        builder.Configuration.GetSection(nameof(GraphiGradeConfig)).Bind(config);

        return config;
    }
}
