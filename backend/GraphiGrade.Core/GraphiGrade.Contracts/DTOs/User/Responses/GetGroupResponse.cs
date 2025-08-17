using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.User.Responses;

public record GetGroupResponse : IResponse
{
    public required string Name { get; set; }

    public required IEnumerable<CommonResourceDto> MembersInGroup { get; set; }

    public required IEnumerable<CommonResourceDto> AvailableExercises { get; set; }
}
