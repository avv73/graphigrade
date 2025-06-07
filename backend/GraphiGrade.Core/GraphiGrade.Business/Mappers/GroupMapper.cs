using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
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

    public UserGroupDto MapToUserGroupDto(Group group)
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

        return new UserGroupDto
        {
            Id = group.Id,
            Name = group.GroupName,
            Uri = linkToGroup ?? string.Empty
        };
    }
}
