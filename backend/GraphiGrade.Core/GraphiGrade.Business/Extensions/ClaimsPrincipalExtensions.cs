using System.Security.Claims;
using GraphiGrade.Business.Authorization;

namespace GraphiGrade.Business.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAdmin(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.IsInRole(Role.AdminRole);
    }
}
