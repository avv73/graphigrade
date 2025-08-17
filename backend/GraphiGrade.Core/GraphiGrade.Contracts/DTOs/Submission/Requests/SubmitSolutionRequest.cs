using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Contracts.DTOs.Submission.Requests;

public record SubmitSolutionRequest
{
    [Required]
    public required string SourceCodeBase64 { get; set; }
}