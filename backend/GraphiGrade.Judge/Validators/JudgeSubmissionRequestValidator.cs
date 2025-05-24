using GraphiGrade.Judge.Common;
using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.DTOs.Enums;
using GraphiGrade.Judge.Validators.Abstractions;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Judge.Validators;

public class JudgeSubmissionRequestValidator : IJudgeSubmissionRequestValidator
{
    private readonly JudgeRunnerSettings _settings;

    public JudgeSubmissionRequestValidator(IOptions<JudgeRunnerSettings> settings)
    {
        _settings = settings.Value;
    }

    public ErrorResult Validate(JudgeSubmissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceCodeBase64))
        {
            return ErrorResultFactory.BuildError(SubmissionErrorCode.InputValidationError, "Source code expected");
        }

        if (string.IsNullOrWhiteSpace(request.ExpectedPatternBase64))
        {
            return ErrorResultFactory.BuildError(SubmissionErrorCode.InputValidationError, "Pattern expected");
        }

        if (request.Timestamp == default)
        {
            return ErrorResultFactory.BuildError(SubmissionErrorCode.InputValidationError, "Timestamp expected");
        }

        if (request.SourceCodeBase64.Length > _settings.MaximumBase64CharsInSubmissionCode)
        {
            return ErrorResultFactory.BuildError(SubmissionErrorCode.ExceedsSizeLimits, "Source code exceeds size limits");
        }

        if (request.ExpectedPatternBase64.Length > _settings.MaximumBase64CharsInResultPattern)
        {
            return ErrorResultFactory.BuildError(SubmissionErrorCode.ExceedsSizeLimits, "Result pattern exceeds size limits");
        }

        if (!ImageDecorator.IsValidImageFromBase64(request.ExpectedPatternBase64))
        {
            return ErrorResultFactory.BuildError(SubmissionErrorCode.InvalidImage, "Result pattern is invalid image");
        }

        return ErrorResultFactory.BuildSuccess();
    }
}
