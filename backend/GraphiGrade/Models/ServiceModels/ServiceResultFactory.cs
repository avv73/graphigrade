using GraphiGrade.DTOs;
using GraphiGrade.DTOs.Abstractions;
using System.Net;

namespace GraphiGrade.Models.ServiceModels;

public static class ServiceResultFactory<T> where T : class, IResponse
{
    public static ServiceResult<T> CreateResult(T result) 
    {
        return new ServiceResult<T>
        {
            Result = result
        };
    }

    public static ServiceResult<T> CreateError(HttpStatusCode code, string? errorDescription = null)
    {
        return new ServiceResult<T>
        {
            Error = new ErrorResponse
            {
                ErrorCode = code,
                ErrorMessage = errorDescription
            }
        };
    }
}
