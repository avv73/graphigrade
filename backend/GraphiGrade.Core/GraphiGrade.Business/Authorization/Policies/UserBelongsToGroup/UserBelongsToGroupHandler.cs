using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Business.Authorization.Policies.UserBelongsToGroup;

public class UserBelongsToGroupHandler : AuthorizationHandler<UserBelongsToGroupRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public UserBelongsToGroupHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserBelongsToGroupRequirement requirement,
        int groupId) // resource is groupId
    {
        var userClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null)
        {
            return;
        }

        string username = userClaim.Value;
        
        Group? group = await _unitOfWork.Groups.GetByIdWithIncludesAsync(groupId, query => query
            .Include(gr => gr.UsersGroups)
                .ThenInclude(ug => ug.User));

        if (group == null)
        {
            return;
        }

        // Check if user belongs to the group.
        if (group.UsersGroups?.Any(u => u.User.Username == username) ?? false)
        {
            context.Succeed(requirement);
        }
    }
}
