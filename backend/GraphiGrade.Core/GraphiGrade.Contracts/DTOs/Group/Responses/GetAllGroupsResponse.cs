using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.User.Responses;

namespace GraphiGrade.Contracts.DTOs.Group.Responses;

public record GetAllGroupsResponse : IResponse
{
    public required IEnumerable<CommonResourceDto> Groups { get; set; }
}
