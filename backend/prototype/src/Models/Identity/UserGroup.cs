using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models.Identity;

public class UserGroup
{
    [Key]
    public int Id { get; set; }

    [MaxLength(20)]
    public required string Name { get; set; } = null!;

    [MaxLength(60)]
    public string? Description { get; set; }

    public ICollection<User>? Users { get; set; }

    public required int ManagerId { get; set; }

    public required User Manager { get; set; }
}
