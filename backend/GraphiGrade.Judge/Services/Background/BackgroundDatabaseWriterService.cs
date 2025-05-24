using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.Infrastructure.Abstractions;
using GraphiGrade.Judge.Messages;
using GraphiGrade.Judge.Repositories.Abstractions;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Judge.Services.Background;

public class BackgroundDatabaseWriterService : BackgroundService
{
    private readonly ISubmissionMessageBus _submissionMessageBus;
    private readonly ISubmissionRepository _repository;

    private readonly int _refreshIntervalMilliseconds;

    public BackgroundDatabaseWriterService(
        ISubmissionMessageBus submissionMessageBus,
        ISubmissionRepository repository,
        IOptions<JudgeRunnerSettings> judgeRunnerSettings)
    {
        _submissionMessageBus = submissionMessageBus;
        _repository = repository;
        _refreshIntervalMilliseconds = judgeRunnerSettings.Value.RefreshIntervalMilliseconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_refreshIntervalMilliseconds, stoppingToken); 

            // Read processed message
            ProcessedSubmissionMessage processedSubmissionMessage = await _submissionMessageBus.PullProcessedSubmissionMessageAsync(stoppingToken);

            // Store submission to database
            // TODO: Use Polly. 
            await _repository.UpdateSubmissionAsync(processedSubmissionMessage.Submission.Id, processedSubmissionMessage.Submission, stoppingToken);
        }
    }
}
