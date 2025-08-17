using GraphiGrade.Contracts.ValidatorAttributes;
using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Contracts.DTOs.Auth.Requests;

public record RegisterRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
    public required string Username { get; set; }

    [Required]
    [PasswordValidation]
    public required string Password { get; set; }
}
