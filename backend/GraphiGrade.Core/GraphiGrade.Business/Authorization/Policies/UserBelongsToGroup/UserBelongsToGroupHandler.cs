using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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
        string username = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        Group? group = await _unitOfWork.Groups.GetByIdAsync(groupId);

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
