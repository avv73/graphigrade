using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Contracts.DTOs.Abstractions;
using System.Net;

namespace GraphiGrade.Business.Authorization.Policies.UserBelongsToGroupOrAdmin;

/// <summary>
/// Authentication requirement for endpoints that authenticates only if:
/// 1. The user accesses a group he belongs to OR
/// 2. The user is admin.
/// </summary>
public class UserBelongsToGroupOrAdminRequirement : IAuthorizationRequirementErrorProducer
{
    public string AdminRole { get; }

    public UserBelongsToGroupOrAdminRequirement(string adminRole)
    {
        AdminRole = adminRole;
    }

    public IResponse ProduceError()
    {
        return ErrorResponseFactory.CreateError(
            HttpStatusCode.Forbidden,
            "You do not belong to this group");
    }
}
