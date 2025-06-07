using System.Net;
using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Business.ServiceModels.Factories;

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
            Error = ErrorResponseFactory.CreateError(code, errorDescription)
        };
    }
}
