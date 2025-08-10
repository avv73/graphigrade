namespace GraphiGrade.Business.ServiceModels.Judge;

public record JudgeBatchRequest
{
    public required string SourceCodeBase64 { get; set; }
    public required string ExpectedPatternBase64 { get; set; }
    public required DateTime Timestamp { get; set; }
}

public record JudgeBatchResponse
{
    public required string SubmissionId { get; set; }
    public required SubmissionStatus Status { get; set; }
    public JudgeSubmissionResult? SubmissionResult { get; set; }
    public required SubmissionErrorCode ErrorCode { get; set; }
    public string? ErrorDetails { get; set; }
    public required DateTime Timestamp { get; set; }
}

public record JudgeSubmissionResult
{
    /// <summary>
    /// Base64 image of the result joined with the pattern
    /// </summary>
    public required string ExecutionResultBase64 { get; set; }
    public required double ExecutionAccuracy { get; set; }
    public required DateTime LastUpdated { get; set; }
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