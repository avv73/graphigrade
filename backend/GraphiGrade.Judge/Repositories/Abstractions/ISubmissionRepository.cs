using GraphiGrade.Judge.Models;

namespace GraphiGrade.Judge.Repositories.Abstractions;

public interface ISubmissionRepository
{
    Task<Submission?> GetBySubmissionIdAsync(string submissionId, CancellationToken cancellationToken = default);

    Task<Submission?> GetQueuedSubmissionAsync(CancellationToken cancellationToken = default);

    Task AddSubmissionAsync(Submission submission, CancellationToken cancellationToken = default);

    Task UpdateSubmissionAsync(string submissionId, Submission submission, CancellationToken cancellationToken = default);
}
