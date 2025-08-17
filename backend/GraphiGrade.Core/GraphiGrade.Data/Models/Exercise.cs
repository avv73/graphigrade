using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Data.Models;

public class  Exercise
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int ExpectedImageId { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVisible { get; set; } 

    public FileMetadata ExpectedImage { get; set; }
    public User CreatedBy { get; set; }
    public ICollection<ExercisesGroups> ExercisesGroups { get; set; }
    public ICollection<Submission> Submissions { get; set; }
}
