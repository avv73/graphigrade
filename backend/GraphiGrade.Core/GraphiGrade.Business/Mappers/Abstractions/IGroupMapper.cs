using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Group.Responses;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers.Abstractions;

public interface IGroupMapper
{
    CommonResourceDto MapToCommonResourceDto(Group group);

    GetGroupResponse MapToGetGroupResponse(Group group, IEnumerable<CommonResourceDto> groupUserList, IEnumerable<CommonResourceDto> groupExerciseList);
    
    CreateGroupResponse MapToCreateGroupResponse(Group group, IEnumerable<CommonResourceDto> assignedUsers);
}
