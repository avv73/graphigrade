using System.Net;
using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Business.Authorization.Policies.UserBelongsToGroup;

/// <summary>
/// Authentication requirement for endpoints that authenticates only if:
/// 1. The user accesses a group he belongs to
/// </summary>
public class UserBelongsToGroupRequirement : IAuthorizationRequirementErrorProducer
{
    public IResponse ProduceError()
    {
        return ErrorResponseFactory.CreateError(
            HttpStatusCode.Forbidden,
            "You do not belong to this group");
    }
}
