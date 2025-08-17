using GraphiGrade.Business.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace GraphiGrade.Business.Authorization.Policies.Admin;

public class AdminHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AdminRequirement requirement)
    {
        if (context.User.IsAdmin())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
