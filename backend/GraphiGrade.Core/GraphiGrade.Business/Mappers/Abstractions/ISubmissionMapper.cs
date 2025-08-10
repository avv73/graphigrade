using GraphiGrade.Business.ServiceModels.Judge;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Submission.Responses;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers.Abstractions;

public interface ISubmissionMapper
{
    CommonResourceDto MapToCommonResourceDto(Submission submission);
    GetSubmissionStatusResponse MapToGetSubmissionStatusResponse(Submission submission, JudgeBatchResponse judgeResponse);
}
