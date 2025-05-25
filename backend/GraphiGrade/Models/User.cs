using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models;

public class User
{
    public required int Id { get; set; }

    [MaxLength(30)]
    public required string Username { get; set; }
    /// <summary>
    /// SHA-256 hash, CHAR(64)
    /// </summary>
    public required string Password { get; set; } 

    public bool IsTeacher { get; set; }

    public required ICollection<Submission> Submissions { get; set; }
    public required ICollection<UsersGroups> UsersGroups { get; set; }
}
