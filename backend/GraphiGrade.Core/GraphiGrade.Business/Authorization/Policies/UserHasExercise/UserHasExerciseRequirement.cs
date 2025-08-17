using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Contracts.DTOs.Abstractions;
using System.Net;

namespace GraphiGrade.Business.Authorization.Policies.UserHasExercise;

public class UserHasExerciseRequirement : IAuthorizationRequirementErrorProducer
{
    public IResponse ProduceError()
    {
        return ErrorResponseFactory.CreateError(
            HttpStatusCode.Forbidden,
            "You are not authorized to perform this action.");
    }
}
