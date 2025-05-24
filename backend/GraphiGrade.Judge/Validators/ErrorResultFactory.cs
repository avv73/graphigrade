using GraphiGrade.Judge.DTOs.Enums;

namespace GraphiGrade.Judge.Validators;

public static class ErrorResultFactory
{
    private static readonly ErrorResult _success = new ErrorResult();

    public static ErrorResult BuildError(SubmissionErrorCode errorCode, string description)
    {
        return new ErrorResult(errorCode, description);
    }

    public static ErrorResult BuildSuccess() => _success;
}
