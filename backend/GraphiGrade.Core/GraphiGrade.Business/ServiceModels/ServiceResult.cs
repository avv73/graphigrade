using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Business.ServiceModels;

public class ServiceResult<T> where T : class, IResponse
{
    public T? Result { get; set; }

    public ErrorResponse? Error { get; set; }

    public bool IsError => Error != null;
}
