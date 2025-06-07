using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Data.Models;

public class FileMetadata
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string StorageUrl { get; set; } = null!;
    public byte Type { get; set; }
    public int Size { get; set; }

    public ICollection<Exercise> ExercisesAsExpectedImage { get; set; }
    public ICollection<Submission> SubmissionsResultImage { get; set; }
    public ICollection<Submission> SubmissionsSourceCode { get; set; }
}
