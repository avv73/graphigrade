using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.Extensions.Logging;

namespace GraphiGrade.Business.Authorization.Policies.UserBelongsToGroupOrAdmin;

public class UserBelongsToGroupOrAdminHandler : AuthorizationHandler<UserBelongsToGroupOrAdminRequirement, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserBelongsToGroupOrAdminHandler> _logger;

    public UserBelongsToGroupOrAdminHandler(IUnitOfWork unitOfWork, ILogger<UserBelongsToGroupOrAdminHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserBelongsToGroupOrAdminRequirement requirement,
        string resource) // resource is groupId
    {
        string? username = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? role = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(username))
        {
            context.Fail();

            return;
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            context.Fail();

            return;
        }

        if (!int.TryParse(resource, out int groupId))
        {
            context.Fail();
            _logger.LogError($"Failed to parse resource to groupId: {resource}");

            return;
        }

        if (role.Equals(requirement.AdminRole, StringComparison.Ordinal))
        {
            context.Succeed(requirement);

            return;
        }

        Group? group = await _unitOfWork.Groups.GetByIdAsync(groupId);

        if (group == null)
        {
            context.Fail();
            return;
        }

        // Check if user belongs to the group.
        if (group.UsersGroups?.Any(u => u.User.Username == username) ?? false)
        {
            context.Succeed(requirement);
        }

        context.Fail();
    }
}
