using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers;

public class UserMapper : IUserMapper
{
    private readonly IExerciseMapper _exerciseMapper;
    private readonly IGroupMapper _groupMapper;

    public UserMapper(IExerciseMapper exerciseMapper, IGroupMapper groupMapper)
    {
        _exerciseMapper = exerciseMapper;
        _groupMapper = groupMapper;
    }

    public GetUserResponse MapToGetUserResponse(User user)
    {
        List<UserExercisesDto> mappedUserExercisesForUser = new();
        List<UserGroupDto> mappedUserGroupsForUser = new();

        if (user.UsersGroups != null)
        {
            foreach (UsersGroups userUsersGroup in user.UsersGroups)
            {
                mappedUserGroupsForUser.Add(_groupMapper.MapToUserGroupDto(userUsersGroup.Group));

                if (userUsersGroup.Group.ExercisesGroups != null)
                {
                    foreach (ExercisesGroups exercisesGroup in userUsersGroup.Group.ExercisesGroups)
                    {
                        mappedUserExercisesForUser.Add(_exerciseMapper.ToUserExercises(exercisesGroup.Exercise));
                    }
                }
            }
        }

        return new GetUserResponse
        {
            Username = user.Username,
            AvailableExercises = mappedUserExercisesForUser,
            MemberInGroups = mappedUserGroupsForUser
        };
    }
}
