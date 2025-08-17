using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Contracts.DTOs.Submission.Requests;
using GraphiGrade.Contracts.DTOs.Submission.Responses;

namespace GraphiGrade.Business.Services.Abstractions;

public interface ISubmissionService
{
    Task<ServiceResult<SubmitSolutionResponse>> SubmitSolutionAsync(int exerciseId, SubmitSolutionRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<GetSubmissionStatusResponse>> GetSubmissionStatusAsync(string submissionId, CancellationToken cancellationToken);
}