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
        var userClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null)
        {
            return Task.CompletedTask;
        }

        string username = userClaim.Value;

        if (username.Equals(resource, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
