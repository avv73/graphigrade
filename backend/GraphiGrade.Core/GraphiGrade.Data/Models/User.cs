using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Data.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(30)]
    [Required]
    public string Username { get; set; } = null!;

    /// <summary>
    /// BCrypt hash, CHAR(60)
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;

    public bool IsTeacher { get; set; }

    public ICollection<Submission>? Submissions { get; set; }
    public ICollection<UsersGroups>? UsersGroups { get; set; }
}
