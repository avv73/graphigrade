using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
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
            action: "GetById",
            controller: "Submission",
            values: new { id = submission.Id });

        return new CommonResourceDto
        {
            Id = submission.Id,
            Name = submission.JudgeId,
            Uri = linkToSubmission ?? string.Empty
        };
    }
}
