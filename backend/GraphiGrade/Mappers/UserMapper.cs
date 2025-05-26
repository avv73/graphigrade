using GraphiGrade.DTOs.Common;
using GraphiGrade.DTOs.User.Responses;
using GraphiGrade.Mappers.Abstractions;
using GraphiGrade.Models;

namespace GraphiGrade.Mappers;

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

        foreach (UsersGroups userUsersGroup in user.UsersGroups)
        {
            foreach (ExercisesGroups exercisesGroup in userUsersGroup.Group.ExercisesGroups)
            {
                mappedUserExercisesForUser.Add(_exerciseMapper.ToUserExercises(exercisesGroup.Exercise));
            }

            mappedUserGroupsForUser.Add(_groupMapper.MapToUserGroupDto(userUsersGroup.Group));
        }

        return new GetUserResponse
        {
            Username = user.Username,
            AvailableExercises = mappedUserExercisesForUser,
            MemberInGroups = mappedUserGroupsForUser
        };
    }
}
