using System.Security.Claims;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IUserResolverService
{
    string Username { get; }

    bool IsAdmin { get; }

    bool Resolve(ClaimsPrincipal userClaimsPrincipal);
}
