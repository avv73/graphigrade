using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GraphiGrade.Business.Authorization.Policies.SameUserOrAdmin;

public class SameUserOrAdminHandler : AuthorizationHandler<SameUserOrAdminRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameUserOrAdminRequirement requirement,
        string resource) // resource is username
    {
        string? username = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? role = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(username))
        {
            context.Fail();

            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            context.Fail();

            return Task.CompletedTask;
        }

        if (role.Equals(requirement.AdminRole, StringComparison.OrdinalIgnoreCase) ||
            username.Equals(resource, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        context.Fail();

        return Task.CompletedTask;
    }
}
