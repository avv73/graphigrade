using GraphiGrade.Judge.DTOs.Enums;

namespace GraphiGrade.Judge.DTOs;

public class JudgeBatchResponse
{
    public string SubmissionId { get; set; } = null!;

    public SubmissionStatus Status { get; set; }

    public JudgeSubmissionResult? SubmissionResult { get; set; }

    public SubmissionErrorCode ErrorCode { get; set; }

    public string? ErrorDetails { get; set; }

    public DateTime Timestamp { get; set; }
}

public class JudgeSubmissionResult
{
    /// <summary>
    /// Base64 image of the result joined with the pattern
    /// </summary>
    public string ExecutionResultBase64 { get; set; }

    public double ExecutionAccuracy { get; set; }

    public DateTime LastUpdated { get; set; }
}
