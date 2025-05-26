using System.ComponentModel.DataAnnotations;
using GraphiGrade.ValidatorAttributes;

namespace GraphiGrade.DTOs.Auth.Requests;

public record RegisterRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
    public required string Username { get; set; }

    [Required]
    [PasswordValidation]
    public required string Password { get; set; }
}
