using System.Net;
using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Contracts.DTOs;

public record ErrorResponse : IResponse
{
    public required HttpStatusCode ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
