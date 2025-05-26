using GraphiGrade.DTOs.Common;
using GraphiGrade.Models;

namespace GraphiGrade.Mappers.Abstractions;

public interface IExerciseMapper
{
    UserExercisesDto ToUserExercises(Exercise exercise);
}
