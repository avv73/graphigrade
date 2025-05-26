using GraphiGrade.DTOs.Common;
using GraphiGrade.Models;

namespace GraphiGrade.Mappers.Abstractions;

public interface IGroupMapper
{
    UserGroupDto MapToUserGroupDto(Group group);
}
