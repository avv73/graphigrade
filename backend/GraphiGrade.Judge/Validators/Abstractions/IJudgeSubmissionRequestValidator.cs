using GraphiGrade.Judge.DTOs;

namespace GraphiGrade.Judge.Validators.Abstractions;

public interface IJudgeSubmissionRequestValidator
{
    ErrorResult Validate(JudgeSubmissionRequest request);
}
