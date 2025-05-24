using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.Models;

namespace GraphiGrade.Judge.Mappers.Abstractions;

public interface IJudgeSubmissionRequestMapper
{
    Submission Map(JudgeSubmissionRequest request);
}
