using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.Infrastructure.Abstractions;
using GraphiGrade.Judge.Messages;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Judge.Services.Background;

public class BackgroundConcurrentProcessorService : BackgroundService
{
    private readonly ISubmissionMessageBus _submissionMessageBus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundConcurrentProcessorService> _logger;

    private readonly int _maxNumberOfThreads;

    public BackgroundConcurrentProcessorService(
        ISubmissionMessageBus submissionMessageBus,
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundConcurrentProcessorService> logger,
        IOptions<JudgeRunnerSettings> judgeRunnerSettings)
    {
        _submissionMessageBus = submissionMessageBus;
        _scopeFactory = scopeFactory;
        _logger = logger;

        _maxNumberOfThreads = judgeRunnerSettings.Value.MaximumRunnerThreads;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Task> tasks = new();

        for (int i = 0; i < _maxNumberOfThreads; i++)
        {
            tasks.Add(Task.Run(() => WorkerLoopAsync(stoppingToken), stoppingToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task WorkerLoopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Judge Background processor thread");
        while (!stoppingToken.IsCancellationRequested)
        {
            UnprocessedSubmissionMessage unprocessedSubmissionMessage = await _submissionMessageBus.PullUnprocessedSubmissionMessageAsync(stoppingToken);

            using var scope = _scopeFactory.CreateScope();

            IJudgeRunnerService judgeRunnerService = scope.ServiceProvider.GetRequiredService<IJudgeRunnerService>();

            Submission? resultSubmission = null;

            try
            {
                resultSubmission = await judgeRunnerService.RunAsync(unprocessedSubmissionMessage.Submission, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"Uncaught exception in WorkerLoop!");
            }

            if (resultSubmission == null)
            {
                continue;
            }

            ProcessedSubmissionMessage processedSubmissionMessage = new(resultSubmission);
            await _submissionMessageBus.PushProcessedSubmissionMessageAsync(
                processedSubmissionMessage,
                stoppingToken);
        }
    }
}
