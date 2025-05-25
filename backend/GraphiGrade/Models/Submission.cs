using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models;

public class Submission
{
    public required int Id { get; set; }
    public required int ExerciseId { get; set; }
    public required int UserId { get; set; }

    /// <summary>
    /// Id from the Judge service, CHAR(36)
    /// </summary>
    public required string JudgeId { get; set; }
    public required byte Status { get; set; }
    public required byte ErrorCode { get; set; }
    public required int SourceCodeId { get; set; }
    public int? ResultImageId { get; set; }
    public decimal? Score { get; set; }
    public required DateTime SubmittedAt { get; set; }
    public DateTime? LastUpdate { get; set; }

    public required Exercise Exercise { get; set; }
    public required User User { get; set; }
    public required FileMetadata SourceCode { get; set; }
    public FileMetadata? ResultImage { get; set; }
}
