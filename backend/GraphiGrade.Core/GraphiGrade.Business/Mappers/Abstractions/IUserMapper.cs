using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers.Abstractions;

public interface IUserMapper
{
    GetUserResponse MapToGetUserResponse(User user, IEnumerable<CommonResourceDto> mappedUserExercisesForUser, IEnumerable<CommonResourceDto> mappedUserGroupsForUser);

    CommonResourceDto MapToCommonResourceDto(User user);
}