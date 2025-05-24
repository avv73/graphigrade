namespace GraphiGrade.Judge.DTOs;

public class JudgeSubmissionRequest
{
    public string SourceCodeBase64 { get; set; }

    public string ExpectedPatternBase64 { get; set; }

    public DateTime Timestamp { get; set; }
}
