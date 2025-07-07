using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GraphiGrade.Business.Authorization.Policies.SameUser;

public class SameUserHandler : AuthorizationHandler<SameUserRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameUserRequirement requirement,
        string resource) // resource is username
    {
        string username = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (username.Equals(resource, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
