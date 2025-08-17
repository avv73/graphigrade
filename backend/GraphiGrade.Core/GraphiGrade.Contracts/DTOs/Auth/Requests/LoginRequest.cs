using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Contracts.DTOs.Auth.Requests;

public record LoginRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
