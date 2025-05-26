using GraphiGrade.DTOs;
using GraphiGrade.DTOs.Auth.Responses.Abstractions;

namespace GraphiGrade.Models.ServiceModels;

public class ServiceResult<T> where T : class, IResponse
{
    public T? Result { get; set; }

    public ErrorResponse? Error { get; set; }

    public bool IsError => Error != null;
}
