using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.Abstractions;
using System.Net;

namespace GraphiGrade.Business.ServiceModels.Factories;

public static class ErrorResponseFactory
{
    public static ErrorResponse CreateError(HttpStatusCode errorCode, string? errorDescription = null)
    {
        return new ErrorResponse
        {
            ErrorCode = errorCode,
            ErrorMessage = errorDescription
        };
    }
}
