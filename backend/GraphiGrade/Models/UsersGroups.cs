using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models;

public class UsersGroups
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public int GroupId { get; set; }

    public User User { get; set; }
    public Group Group { get; set; }
}
