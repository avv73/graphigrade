using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.Exercise.Responses;

public record AssignExerciseToGroupResponse : IResponse
{
    public required CommonResourceDto Exercise { get; set; }
    public required CommonResourceDto AssignedGroup { get; set; }
}
