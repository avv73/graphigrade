using System.Security.Claims;
using GraphiGrade.Business.Extensions;
using GraphiGrade.Business.Services.Abstractions;

namespace GraphiGrade.Business.Services;

public class UserResolverService : IUserResolverService
{
    public string Username { get; private set; } = string.Empty;

    public bool IsAdmin { get; private set; }

    public bool Resolve(ClaimsPrincipal userClaimsPrincipal)
    {
        string? username = userClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        Username = username;
        IsAdmin = userClaimsPrincipal.IsAdmin();

        return true;
    }
}
