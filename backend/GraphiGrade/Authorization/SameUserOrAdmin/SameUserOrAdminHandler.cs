using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GraphiGrade.Authorization.SameUserOrAdmin;

public class SameUserOrAdminHandler : AuthorizationHandler<SameUserOrAdminRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        SameUserOrAdminRequirement requirement,
        string resource)
    {
        string? username = context.User.FindFirst(ClaimTypes.Name)?.Value;
        string? role = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return Task.CompletedTask;
        }

        if (role.Equals(requirement.AdminRole, StringComparison.OrdinalIgnoreCase) ||
            username.Equals(resource, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
