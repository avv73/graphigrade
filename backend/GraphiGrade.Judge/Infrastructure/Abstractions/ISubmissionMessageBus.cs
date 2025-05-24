using GraphiGrade.Judge.Messages;

namespace GraphiGrade.Judge.Infrastructure.Abstractions;

public interface ISubmissionMessageBus
{
    ValueTask PushUnprocessedSubmissionMessageAsync(UnprocessedSubmissionMessage unprocessedSubmissionMessage, CancellationToken cancellationToken);
    ValueTask<UnprocessedSubmissionMessage> PullUnprocessedSubmissionMessageAsync(CancellationToken cancellationToken);
    ValueTask PushProcessedSubmissionMessageAsync(ProcessedSubmissionMessage processedSubmissionMessage, CancellationToken cancellationToken);
    ValueTask<ProcessedSubmissionMessage> PullProcessedSubmissionMessageAsync(CancellationToken cancellationToken);
}