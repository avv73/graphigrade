using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Contracts.DTOs.Exercise.Requests;

public record CreateExerciseRequest
{
    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    public bool IsVisible { get; set; }

    [Required]
    public required IEnumerable<int> GroupIds { get; set; }

    [Required]
    public required string ExpectedImageBase64 { get; set; }
}
