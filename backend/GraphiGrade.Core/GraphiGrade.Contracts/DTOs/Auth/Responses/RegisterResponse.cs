using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Contracts.DTOs.Auth.Responses;

public record RegisterResponse : IResponse
{
    public required string Username { get; set; }

    public string? JwtToken { get; set; }
}
