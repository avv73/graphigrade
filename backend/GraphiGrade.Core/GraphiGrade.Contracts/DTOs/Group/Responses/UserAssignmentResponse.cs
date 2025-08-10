using GraphiGrade.Contracts.DTOs.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Contracts.DTOs.Group.Responses;

public record UserAssignmentResponse : IResponse
{
    public required IEnumerable<CommonResourceDto> AssignedUsers { get; set; }
    
    public required IEnumerable<string> Errors { get; set; }
}