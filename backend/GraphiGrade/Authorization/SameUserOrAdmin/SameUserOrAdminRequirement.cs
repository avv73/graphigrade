using Microsoft.AspNetCore.Authorization;

namespace GraphiGrade.Authorization.SameUserOrAdmin;

/// <summary>
/// Authentication requirement for endpoints that authenticates only if:
/// 1. The user accesses his own resource OR
/// 2. The user is admin.
/// </summary>
public class SameUserOrAdminRequirement : IAuthorizationRequirement
{
    public string AdminRole { get; }

    public SameUserOrAdminRequirement(string adminRole)
    {
        AdminRole = adminRole;
    }    
}

