using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GraphiGrade.Business.Mappers;

public class UserMapper : IUserMapper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserMapper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public GetUserResponse MapToGetUserResponse(User user, 
        IEnumerable<CommonResourceDto> mappedUserExercisesForUser,
        IEnumerable<CommonResourceDto> mappedUserGroupsForUser)
    {
        return new GetUserResponse
        {
            Username = user.Username,
            AvailableExercises = mappedUserExercisesForUser,
            MemberInGroups = mappedUserGroupsForUser
        };
    }

    public CommonResourceDto MapToCommonResourceDto(User user)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new NullReferenceException("Expected HttpContext to be not null.");
        }

        string? linkToUser = _linkGenerator.GetUriByAction(
            _httpContextAccessor.HttpContext,
            action: "GetByUsername",
            controller: "User",
            values: new { username = user.Username });

        return new CommonResourceDto
        {
            Id = user.Id,
            Name = user.Username,
            Uri = linkToUser ?? string.Empty
        };
    }
}
