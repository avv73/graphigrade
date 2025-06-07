using GraphiGrade.Contracts.DTOs.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace GraphiGrade.Business.Authorization.Policies.Abstractions;

public interface IAuthorizationRequirementErrorProducer : IAuthorizationRequirement
{
    IResponse ProduceError();
}
