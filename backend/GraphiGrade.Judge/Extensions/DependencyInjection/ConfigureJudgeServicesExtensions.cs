using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.Infrastructure;
using GraphiGrade.Judge.Infrastructure.Abstractions;
using GraphiGrade.Judge.Mappers;
using GraphiGrade.Judge.Mappers.Abstractions;
using GraphiGrade.Judge.Services;
using GraphiGrade.Judge.Services.Abstractions;
using GraphiGrade.Judge.Services.Background;
using GraphiGrade.Judge.Services.OverlapStrategies;
using GraphiGrade.Judge.Services.OverlapStrategies.Abstractions;
using GraphiGrade.Judge.Validators;
using GraphiGrade.Judge.Validators.Abstractions;

namespace GraphiGrade.Judge.Extensions.DependencyInjection;

public static class ConfigureJudgeServicesExtensions
{
    public static void AddJudgeServices(this IServiceCollection services, IConfiguration config)
    {
        // Configs
        services.Configure<JudgeRunnerSettings>(
            config.GetSection("JudgeRunnerSettings"));

        // Infrastructure
        services.AddSingleton<ISubmissionMessageBus, SubmissionMessageBus>();

        // Background Services
        services.AddHostedService<BackgroundDatabaseReaderService>();
        services.AddHostedService<BackgroundDatabaseWriterService>();
        services.AddHostedService<BackgroundConcurrentProcessorService>();

        // Validators
        services.AddSingleton<IJudgeSubmissionRequestValidator, JudgeSubmissionRequestValidator>();

        // Mappers
        services.AddSingleton<IJudgeSubmissionRequestMapper, JudgeSubmissionRequestMapper>();
        services.AddSingleton<IJudgeBatchResponseMapper, JudgeBatchResponseMapper>();

        // Services
        services.AddSingleton<ISubmissionsBatchService, SubmissionsBatchService>();
        services.AddSingleton<IJudgeRunnerService, JudgeRunnerService>();
        services.AddSingleton<IJudgeExecutorService, JudgeExecutorService>();
        services.AddSingleton<IJudgeCompilerService, JudgeCompilerService>();
        services.AddSingleton<IJudgeSourceCodeAnalyzerService, JudgeSourceCodeAnalyzerService>();

        // Judge Strategy
        services.AddSingleton<IJudgeOverlapStrategy, DiscreteJudgeOverlapStrategy>();
    }
}
