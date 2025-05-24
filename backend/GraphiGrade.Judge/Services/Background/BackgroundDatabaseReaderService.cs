using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.DTOs.Enums;
using GraphiGrade.Judge.Infrastructure.Abstractions;
using GraphiGrade.Judge.Messages;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Repositories.Abstractions;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Judge.Services.Background;

public class BackgroundDatabaseReaderService : BackgroundService
{
    private readonly ISubmissionMessageBus _submissionMessageBus;
    private readonly ISubmissionRepository _repository;

    private readonly int _refreshIntervalMilliseconds;

    public BackgroundDatabaseReaderService(
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

            // Read for submission messages in the queue 
            // TODO: Use POLLY here
            Submission? queuedSubmission = await _repository.GetQueuedSubmissionAsync(stoppingToken);

            if (queuedSubmission == null)
            {
                continue;
            }

            UnprocessedSubmissionMessage messageToProcessor = new(queuedSubmission);

            await _submissionMessageBus.PushUnprocessedSubmissionMessageAsync(messageToProcessor, stoppingToken);

            queuedSubmission.Status = SubmissionStatus.Running;
            // TODO: Use POLLY here
            await _repository.UpdateSubmissionAsync(queuedSubmission.Id, queuedSubmission, stoppingToken);
        }
    }
}

