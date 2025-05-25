using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models;

public class Exercise
{
    public required int Id { get; set; }

    [MaxLength(100)]
    public required string Title { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }

    public required int ExpectedImageId { get; set; }
    public required int CreatedById { get; set; }
    public required DateTime CreatedAt { get; set; }
    public bool IsVisible { get; set; }

    public required FileMetadata ExpectedImage { get; set; }
    public required User CreatedBy { get; set; }
    public required ICollection<ExercisesGroups> ExercisesGroups { get; set; }
    public required ICollection<Submission> Submissions { get; set; }
}
