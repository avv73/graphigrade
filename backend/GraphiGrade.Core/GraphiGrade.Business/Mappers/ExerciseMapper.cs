using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;
using GraphiGrade.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GraphiGrade.Business.Mappers;

public class ExerciseMapper : IExerciseMapper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExerciseMapper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public CommonResourceDto MapToCommonResourceDto(Exercise exercise)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new NullReferenceException("Expected HttpContext to be not null.");
        }

        string? linkToExercise = _linkGenerator.GetUriByAction(
            _httpContextAccessor.HttpContext,
            action: "GetExerciseById",
            controller: "Exercise",
            values: new { id = exercise.Id });

        return new CommonResourceDto
        {
            Id = exercise.Id,
            Name = exercise.Title,
            Uri = linkToExercise ?? string.Empty
        };
    }

    public GetExerciseResponse MapToGetExerciseResponse(
        Exercise exercise, 
        string imageBlobUrl, 
        CommonResourceDto createdByUser,
        IEnumerable<CommonResourceDto> submissions)
    {
        return new GetExerciseResponse
        {
            Title = exercise.Title,
            Description = exercise.Description ?? string.Empty,
            CreatedAt = exercise.CreatedAt,
            CreatedByUser = createdByUser,
            ImageBlobUrl = imageBlobUrl,
            Submissions = submissions
        };
    }
}
