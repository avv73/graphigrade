using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.DTOs.Enums;
using GraphiGrade.Judge.Mappers.Abstractions;
using GraphiGrade.Judge.Models;

namespace GraphiGrade.Judge.Mappers;

public class JudgeSubmissionRequestMapper : IJudgeSubmissionRequestMapper
{
    public Submission Map(JudgeSubmissionRequest request)
    {
        return new Submission
        {
            Id = Guid.NewGuid().ToString(),
            SourceCodeBase64 = request.SourceCodeBase64,
            ExpectedPatternBase64 = request.ExpectedPatternBase64,
            Status = SubmissionStatus.Queued,
            LastUpdated = DateTime.Now
        };
    }
}
