using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.Repositories;
using GraphiGrade.Judge.Repositories.Abstractions;

namespace GraphiGrade.Judge.Extensions.DependencyInjection;

public static class ConfigureDatabaseExtensions
{
    public static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JudgeDatabaseSettings>(
            builder.Configuration.GetSection("JudgeDatabase"));

        builder.Services.AddSingleton<ISubmissionRepository, SubmissionsRepository>();
    }
}
