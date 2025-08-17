using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.User.Responses;

public record StudentWithGroupsDto
{
    public required string Username { get; set; }
    public required int Id { get; set; }
    public required IEnumerable<CommonResourceDto> Groups { get; set; }
}

public record GetAllStudentsResponse : IResponse
{
    public required IEnumerable<StudentWithGroupsDto> Users { get; set; }
}
