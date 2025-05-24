using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.DTOs.Enums;
using System.Net;

namespace GraphiGrade.Judge.Extensions;

public static class JudgeSubmissionRequestExtensions
{
    public static HttpStatusCode MapToHttpStatusCode(this JudgeBatchResponse response, bool returnAcceptedInsteadOK = false)
    {
        switch (response.ErrorCode)
        {
            case SubmissionErrorCode.None:
                return returnAcceptedInsteadOK ? HttpStatusCode.Accepted : HttpStatusCode.OK;

            case SubmissionErrorCode.ExceedsSizeLimits:
            case SubmissionErrorCode.InvalidImage:
            case SubmissionErrorCode.InputValidationError:
                return HttpStatusCode.BadRequest;

            case SubmissionErrorCode.SubmissionNotFound:
                return HttpStatusCode.NotFound;

            case SubmissionErrorCode.UnknownProcessingError:
                return HttpStatusCode.InternalServerError;

            case SubmissionErrorCode.FlaggedAsSuspicious:
            case SubmissionErrorCode.CompilationFailed:
            case SubmissionErrorCode.ExecutionFailed:
            case SubmissionErrorCode.CapturingError:
            case SubmissionErrorCode.UnknownExecutionError:
                return HttpStatusCode.OK;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
