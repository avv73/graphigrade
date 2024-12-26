using Microsoft.AspNetCore.Identity;

namespace GraphiGrade.Models.Identity;

public class User : IdentityUser<int>
{
    public ICollection<UserGroup>? UserGroups { get; set; }
    public ICollection<UserGroup>? ManagesGroups { get; set; }
}