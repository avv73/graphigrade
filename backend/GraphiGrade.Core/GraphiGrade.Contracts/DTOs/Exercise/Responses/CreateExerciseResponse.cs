using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.Exercise.Responses;

public record CreateExerciseResponse : IResponse
{
    public required int Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string ImageBlobUrl { get; set; }

    public required CommonResourceDto CreatedByUser { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required IEnumerable<CommonResourceDto> Submissions { get; set; }
}
