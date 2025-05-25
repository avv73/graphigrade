namespace GraphiGrade.Models;

public class UsersGroups
{
    public required int UserId { get; set; }
    public required int GroupId { get; set; }

    public required User User { get; set; }
    public required Group Group { get; set; }
}
