using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Data.Models;

public class Group
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string GroupName { get; set; } = null!;

    public ICollection<ExercisesGroups>? ExercisesGroups { get; set; } = null!;
    public ICollection<UsersGroups>? UsersGroups { get; set; } = null!;
}
