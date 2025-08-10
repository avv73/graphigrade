using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Group.Responses;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GraphiGrade.Business.Mappers;

public class GroupMapper : IGroupMapper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GroupMapper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public CommonResourceDto MapToUserGroupDto(Group group)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new NullReferenceException("Expected HttpContext to be not null.");
        }

        string? linkToGroup = _linkGenerator.GetUriByAction(
            _httpContextAccessor.HttpContext,
            action: "GetGroupByIdAsync",
            controller: "Group",
            values: new { id = group.Id });

        return new CommonResourceDto
        {
            Id = group.Id,
            Name = group.GroupName,
            Uri = linkToGroup ?? string.Empty
        };
    }

    public GetGroupResponse MapToGetGroupResponse(
        Group group, 
        IEnumerable<CommonResourceDto> groupUserList, 
        IEnumerable<CommonResourceDto> groupExerciseList)
    {
        return new GetGroupResponse
        {
            Name = group.GroupName,
            MembersInGroup = groupUserList,
            AvailableExercises = groupExerciseList
        };
    }

    public CreateGroupResponse MapToCreateGroupResponse(Group group, IEnumerable<CommonResourceDto> assignedUsers)
    {
        return new CreateGroupResponse
        {
            Id = group.Id,
            GroupName = group.GroupName,
            CreatedAt = DateTime.UtcNow,
            AssignedUsers = assignedUsers
        };
    }
}
