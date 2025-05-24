using System.Threading.Channels;
using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.Infrastructure.Abstractions;
using GraphiGrade.Judge.Messages;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Judge.Infrastructure;

public class SubmissionMessageBus : ISubmissionMessageBus
{
    private readonly Channel<UnprocessedSubmissionMessage> _unprocessedSubmissionMessagesChannel;
    private readonly Channel<ProcessedSubmissionMessage> _processedSubmissionMessagesChannel;

    public SubmissionMessageBus(IOptions<JudgeRunnerSettings> judgeRunnerSettings)
    {
        _unprocessedSubmissionMessagesChannel =
            Channel.CreateBounded<UnprocessedSubmissionMessage>(judgeRunnerSettings.Value.UnprocessedSubmissionChannelMaximumMessages);
        _processedSubmissionMessagesChannel =
            Channel.CreateBounded<ProcessedSubmissionMessage>(judgeRunnerSettings.Value.ProcessedSubmissionChannelMaximumMessages);
    }

    public ValueTask PushUnprocessedSubmissionMessageAsync(UnprocessedSubmissionMessage unprocessedSubmissionMessage, CancellationToken cancellationToken)
    {
        return _unprocessedSubmissionMessagesChannel.Writer.WriteAsync(unprocessedSubmissionMessage, cancellationToken);
    }

    public ValueTask<UnprocessedSubmissionMessage> PullUnprocessedSubmissionMessageAsync(CancellationToken cancellationToken)
    {
        return _unprocessedSubmissionMessagesChannel.Reader.ReadAsync(cancellationToken);
    }

    public ValueTask PushProcessedSubmissionMessageAsync(ProcessedSubmissionMessage processedSubmissionMessage, CancellationToken cancellationToken)
    {
        return _processedSubmissionMessagesChannel.Writer.WriteAsync(processedSubmissionMessage, cancellationToken);
    }

    public ValueTask<ProcessedSubmissionMessage> PullProcessedSubmissionMessageAsync(CancellationToken cancellationToken)
    {
        return _processedSubmissionMessagesChannel.Reader.ReadAsync(cancellationToken);
    }
}
