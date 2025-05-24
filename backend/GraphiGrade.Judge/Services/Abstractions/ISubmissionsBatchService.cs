using GraphiGrade.Judge.DTOs;

namespace GraphiGrade.Judge.Services.Abstractions;

public interface ISubmissionsBatchService
{
    Task<JudgeBatchResponse> BatchSubmissionAsync(JudgeSubmissionRequest request, CancellationToken cancellationToken);

    Task<JudgeBatchResponse> GetSubmissionAsync(string submissionId, CancellationToken cancellationToken);
}
