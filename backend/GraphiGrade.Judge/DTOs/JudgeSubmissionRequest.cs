namespace GraphiGrade.Judge.DTOs;

public record JudgeSubmissionRequest
{
    public string SourceCodeBase64 { get; set; }

    public string ExpectedPatternBase64 { get; set; }

    public DateTime Timestamp { get; set; }
}
