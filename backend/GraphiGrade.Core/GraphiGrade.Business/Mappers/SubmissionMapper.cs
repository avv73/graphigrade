using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Business.ServiceModels.Judge;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Submission.Responses;
using GraphiGrade.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GraphiGrade.Business.Mappers;

public class SubmissionMapper : ISubmissionMapper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubmissionMapper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public CommonResourceDto MapToCommonResourceDto(Submission submission)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new NullReferenceException("Expected HttpContext to be not null.");
        }

        string? linkToSubmission = _linkGenerator.GetUriByAction(
            _httpContextAccessor.HttpContext,
            action: "GetSubmissionStatusAsync",
            controller: "Submission",
            values: new { submissionId = submission.JudgeId });

        return new CommonResourceDto
        {
            Id = submission.Id,
            Name = submission.JudgeId,
            Uri = linkToSubmission ?? string.Empty
        };
    }

    public GetSubmissionStatusResponse MapToGetSubmissionStatusResponse(Submission submission, JudgeBatchResponse judgeResponse)
    {
        return new GetSubmissionStatusResponse
        {
            SubmissionId = judgeResponse.SubmissionId,
            Status = (Contracts.DTOs.Submission.Responses.SubmissionStatus)judgeResponse.Status,
            SubmissionResult = judgeResponse.SubmissionResult != null 
                ? new Contracts.DTOs.Submission.Responses.SubmissionResult
                {
                    ExecutionResultBase64 = judgeResponse.SubmissionResult.ExecutionResultBase64,
                    ExecutionAccuracy = judgeResponse.SubmissionResult.ExecutionAccuracy,
                    LastUpdated = judgeResponse.SubmissionResult.LastUpdated
                }
                : null,
            ErrorCode = (Contracts.DTOs.Submission.Responses.SubmissionErrorCode)judgeResponse.ErrorCode,
            ErrorDetails = judgeResponse.ErrorDetails,
            Timestamp = judgeResponse.Timestamp,
            SubmittedAt = submission.SubmittedAt,
            LastUpdate = submission.LastUpdate
        };
    }
}
