using GraphiGrade.Contracts.DTOs.Abstractions;

namespace GraphiGrade.Contracts.DTOs.Auth.Responses;

public record LoginResponse : IResponse
{
    public required string Username { get; set; }

    public required string JwtToken { get; set; }
}
