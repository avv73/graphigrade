using System.Net;

namespace GraphiGrade.DTOs;

public record ErrorResponse
{
    public required HttpStatusCode ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
