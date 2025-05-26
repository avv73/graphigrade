using GraphiGrade.DTOs.Abstractions;
using GraphiGrade.DTOs.Common;

namespace GraphiGrade.DTOs.User.Responses;

public record GetUserResponse : IResponse
{
    public required string Username { get; set; }

    public required ICollection<UserGroupDto> MemberInGroups { get; set; }

    public required ICollection<UserExercisesDto> AvailableExercises { get; set; }
}
