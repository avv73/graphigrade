using GraphiGrade.DTOs.Common;
using GraphiGrade.Mappers.Abstractions;
using GraphiGrade.Models;

namespace GraphiGrade.Mappers;

public class ExerciseMapper : IExerciseMapper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExerciseMapper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public UserExercisesDto ToUserExercises(Exercise exercise)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new NullReferenceException("Expected HttpContext to be not null.");
        }

        string? linkToExercise = _linkGenerator.GetUriByAction(
            _httpContextAccessor.HttpContext,
            action: "GetExerciseByIdAsync",
            controller: "Exercise",
            values: new { id = exercise.Id });

        return new UserExercisesDto
        {
            Id = exercise.Id,
            Name = exercise.Title,
            Uri = linkToExercise ?? string.Empty
        };
    }
}
