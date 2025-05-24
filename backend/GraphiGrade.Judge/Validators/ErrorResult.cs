using GraphiGrade.Judge.DTOs.Enums;

namespace GraphiGrade.Judge.Validators;

public class ErrorResult
{
    public SubmissionErrorCode ErrorCode { get; set; }

    public string? Description { get; set; }

    public bool IsValid => ErrorCode == SubmissionErrorCode.None;

    public ErrorResult()
    {
        
    }

    public ErrorResult(SubmissionErrorCode errorCode, string description)
    {
        ErrorCode = errorCode;
        Description = description;
    }
}
