using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Contracts.DTOs.Submission.Responses;

public record GetUserSubmissionDto
{
    public required int Id { get; set; }
    public required DateTime SubmittedAt { get; set; }
    public decimal? Score { get; set; }
    public required SubmissionStatus Status { get; set; }
}

public record GetUserSubmissionsResponse : IResponse
{
    public required IEnumerable<GetUserSubmissionDto> Submissions { get; set; }
}
