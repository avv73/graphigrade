using GraphiGrade.DTOs.Auth.Responses.Abstractions;

namespace GraphiGrade.DTOs.Auth.Responses;

public record RegisterResponse : IResponse
{
    public required string Username { get; set; }
    
    public string? JwtToken { get; set; }
}
