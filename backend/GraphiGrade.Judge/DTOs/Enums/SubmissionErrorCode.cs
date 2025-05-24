namespace GraphiGrade.Judge.DTOs.Enums;

public enum SubmissionErrorCode
{
    None = 0,
    UnknownProcessingError,
    ExceedsSizeLimits,
    InvalidImage,
    InputValidationError,
    SubmissionNotFound,
    FlaggedAsSuspicious,
    CompilationFailed,
    ExecutionFailed,
    CapturingError,
    UnknownExecutionError,
}
