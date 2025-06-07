using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Contracts.DTOs.Abstractions;
using System.Net;

namespace GraphiGrade.Business.Authorization.Policies.SameUserOrAdmin;

/// <summary>
/// Authentication requirement for endpoints that authenticates only if:
/// 1. The user accesses his own resource OR
/// 2. The user is admin.
/// </summary>
public class SameUserOrAdminRequirement : IAuthorizationRequirementErrorProducer
{
    public string AdminRole { get; }

    public SameUserOrAdminRequirement(string adminRole)
    {
        AdminRole = adminRole;
    }

    public IResponse ProduceError()
    {
        return ErrorResponseFactory.CreateError(
            HttpStatusCode.Forbidden,
            "You are not the user trying to access this page");
    }
}

