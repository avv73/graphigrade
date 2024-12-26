using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models.Identity;

public class UserGroup
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<User>? Users { get; set; }
    public int ManagerId { get; set; }
    public User Manager { get; set; } = null!;
}
