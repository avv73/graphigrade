using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.Group.Responses;

public record CreateGroupResponse : IResponse
{
    public required int Id { get; set; }

    public required string GroupName { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required IEnumerable<CommonResourceDto> AssignedUsers { get; set; }
}