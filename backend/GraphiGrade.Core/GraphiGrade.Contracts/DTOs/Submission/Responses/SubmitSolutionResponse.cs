using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Contracts.DTOs.Submission.Responses;

public record SubmitSolutionResponse : IResponse
{
    public required string SubmissionId { get; set; }
    public required SubmissionStatus Status { get; set; }
    public required DateTime SubmittedAt { get; set; }
}

public enum SubmissionStatus
{
    NotQueued,
    Queued,
    Running,
    Finished
}

public enum SubmissionErrorCode
{
    None = 0,
    UnknownProcessingError,
    ExceedsSizeLimits,
    InvalidImage,
    InputValidationError,
    SubmissionNotFound,
    FlaggedAsSuspicious,
    CompilationFailed,
    ExecutionFailed,
    CapturingError,
    UnknownExecutionError,
}