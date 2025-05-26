using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models;

public class Submission
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public int UserId { get; set; }

    /// <summary>
    /// Id from the Judge service, CHAR(36)
    /// </summary>
    [Required]
    public string JudgeId { get; set; } = null!;
    public byte Status { get; set; }
    public byte ErrorCode { get; set; }
    public int SourceCodeId { get; set; }
    public int? ResultImageId { get; set; }
    public decimal? Score { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? LastUpdate { get; set; }

    public Exercise Exercise { get; set; }
    public User User { get; set; }
    public FileMetadata SourceCode { get; set; }
    public FileMetadata? ResultImage { get; set; }
}
