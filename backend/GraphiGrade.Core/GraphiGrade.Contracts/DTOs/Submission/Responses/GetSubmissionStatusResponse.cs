using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Contracts.DTOs.Submission.Responses;

public record GetSubmissionStatusResponse : IResponse
{
    public required string SubmissionId { get; set; }
    public required SubmissionStatus Status { get; set; }
    public SubmissionResult? SubmissionResult { get; set; }
    public required SubmissionErrorCode ErrorCode { get; set; }
    public string? ErrorDetails { get; set; }
    public required DateTime Timestamp { get; set; }
    public required DateTime SubmittedAt { get; set; }
    public required DateTime? LastUpdate { get; set; }
}

public record SubmissionResult
{
    /// <summary>
    /// Base64 image of the result joined with the pattern
    /// </summary>
    public required string ExecutionResultBase64 { get; set; }
    public required double ExecutionAccuracy { get; set; }
    public required DateTime LastUpdated { get; set; }
}