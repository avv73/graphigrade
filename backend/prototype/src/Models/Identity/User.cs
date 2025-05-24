using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Models.Identity;

[Index(nameof(FacultyNumber), IsUnique = true)]
public class User : IdentityUser<int>
{
    public ICollection<UserGroup>? UserGroups { get; set; }

    public ICollection<UserGroup>? ManagesGroups { get; set; }

    [MaxLength(10)]
    public required string FacultyNumber { get; set; }
}