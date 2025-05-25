using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models;

public class Group
{
    public required int Id { get; set; }

    [MaxLength(30)]
    public required string GroupName { get; set; }

    public required ICollection<ExercisesGroups> ExercisesGroups { get; set; }
    public required ICollection<UsersGroups> UsersGroups { get; set; }
}
