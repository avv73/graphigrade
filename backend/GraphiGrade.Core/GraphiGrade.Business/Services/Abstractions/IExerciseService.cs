using System.Security.Claims;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IExerciseService
{
    Task<ServiceResult<GetExerciseResponse>> GetExerciseByIdAsync(
        int id, 
        ClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken);
}
