using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models;

public class FileMetadata
{
    public required int Id { get; set; }

    [MaxLength(100)]
    public required string StorageUrl { get; set; }
    public required byte Type { get; set; }
    public required int Size { get; set; }

    public required ICollection<Exercise> ExercisesAsExpectedImage { get; set; }
    public required ICollection<Submission> SubmissionsResultImage { get; set; }
    public required ICollection<Submission> SubmissionsSourceCode { get; set; }
}
