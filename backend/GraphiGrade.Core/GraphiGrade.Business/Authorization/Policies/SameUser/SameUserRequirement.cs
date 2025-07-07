using System.Net;
using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Business.Authorization.Policies.SameUser;

/// <summary>
/// Authentication requirement for endpoints that authenticates only if:
/// 1. The user accesses his own resource
/// </summary>
public class SameUserRequirement : IAuthorizationRequirementErrorProducer
{
    public IResponse ProduceError()
    {
        return ErrorResponseFactory.CreateError(
            HttpStatusCode.Forbidden,
            "You are not the user trying to access this page");
    }
}

