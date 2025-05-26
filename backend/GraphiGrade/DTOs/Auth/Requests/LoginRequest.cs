using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.DTOs.Auth.Requests;

public record LoginRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
