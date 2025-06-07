using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers.Abstractions;

public interface IGroupMapper
{
    UserGroupDto MapToUserGroupDto(Group group);
}
