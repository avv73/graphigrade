using GraphiGrade.Business.ServiceModels.Judge;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IJudgeService
{
    Task<JudgeBatchResponse?> SubmitForJudgingAsync(JudgeBatchRequest request, CancellationToken cancellationToken);
    Task<JudgeBatchResponse?> GetSubmissionStatusAsync(string submissionId, CancellationToken cancellationToken);
}