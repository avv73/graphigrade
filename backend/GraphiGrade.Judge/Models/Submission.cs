using GraphiGrade.Judge.DTOs.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace GraphiGrade.Judge.Models;

public class Submission
{
    [BsonId]
    public string Id { get; set; } = null!;
    
    public string SourceCodeBase64 { get; set; } = null!;

    public string ExpectedPatternBase64 { get; set; } = null!;

    public string? ExecutionResultBase64 { get; set; } = null!;

    public double ExecutionAccuracy { get; set; }

    public SubmissionStatus Status { get; set; }

    public SubmissionErrorCode ErrorCode { get; set; }

    public string? ErrorDetails { get; set; } = null!;

    public DateTime LastUpdated { get; set; }
}
