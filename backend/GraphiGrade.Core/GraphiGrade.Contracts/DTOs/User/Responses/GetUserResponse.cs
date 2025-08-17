using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.User.Responses;

public record GetUserResponse : IResponse
{
    public required string Username { get; set; }

    public required IEnumerable<CommonResourceDto> MemberInGroups { get; set; }

    public required IEnumerable<CommonResourceDto> AvailableExercises { get; set; }
}
