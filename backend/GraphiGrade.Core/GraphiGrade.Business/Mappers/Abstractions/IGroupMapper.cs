using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers.Abstractions;

public interface IGroupMapper
{
    CommonResourceDto MapToUserGroupDto(Group group);

    GetGroupResponse MapToGetGroupResponse(Group group, IEnumerable<CommonResourceDto> groupUserList, IEnumerable<CommonResourceDto> groupExerciseList);
}
