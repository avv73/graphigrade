using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.User.Responses;

public record GetUnassignedUsersResponse : IResponse
{
    public required IEnumerable<CommonResourceDto> Users { get; set; }
}
