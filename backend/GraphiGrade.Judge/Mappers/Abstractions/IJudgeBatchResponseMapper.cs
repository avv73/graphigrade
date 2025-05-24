using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Validators;

namespace GraphiGrade.Judge.Mappers.Abstractions;

public interface IJudgeBatchResponseMapper
{
    JudgeBatchResponse Map(ErrorResult validationResult);

    JudgeBatchResponse Map(Submission submission);
}
