using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.Mappers.Abstractions;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Validators;

namespace GraphiGrade.Judge.Mappers;

public class JudgeBatchResponseMapper : IJudgeBatchResponseMapper
{
    public JudgeBatchResponse Map(ErrorResult validationResult)
    {
        return new JudgeBatchResponse
        {
            ErrorCode = validationResult.ErrorCode,
            ErrorDetails = validationResult.Description!,
            Timestamp = DateTime.Now
        };
    }

    public JudgeBatchResponse Map(Submission submission)
    {
        return new JudgeBatchResponse
        {
            SubmissionId = submission.Id,
            ErrorCode = submission.ErrorCode,
            ErrorDetails = submission.ErrorDetails,
            Status = submission.Status,
            SourceCodeBase64 = submission.SourceCodeBase64,
            SubmissionResult = MapSubmissionResult(submission),
            Timestamp = DateTime.Now
        };
    }

    private JudgeSubmissionResult? MapSubmissionResult(Submission submission)
    {
        if (string.IsNullOrWhiteSpace(submission.ExecutionResultBase64))
        {
            return null;
        }

        return new JudgeSubmissionResult
        {
            ExecutionAccuracy = submission.ExecutionAccuracy,
            ExecutionResultBase64 = submission.ExecutionResultBase64,
            LastUpdated = submission.LastUpdated
        };
    }
}
