using System.Security.Claims;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Contracts.DTOs.Exercise.Requests;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IExerciseService
{
    Task<ServiceResult<GetExerciseResponse>> GetExerciseByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ServiceResult<CreateExerciseResponse>> CreateExerciseAsync(
        CreateExerciseRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<AssignExerciseToGroupResponse>> AssignExerciseToGroupAsync(
        int exerciseId,
        int groupId,
        CancellationToken cancellationToken);
}
