using GraphiGrade.Judge.Models;

namespace GraphiGrade.Judge.Services.Abstractions;

public interface IJudgeRunnerService
{
    Task<Submission> RunAsync(Submission submission, CancellationToken cancellationToken);
}
