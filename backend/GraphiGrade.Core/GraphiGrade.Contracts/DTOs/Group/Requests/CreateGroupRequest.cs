using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Contracts.DTOs.Group.Requests;

public record CreateGroupRequest
{
    [Required]
    [MaxLength(30)]
    public required string GroupName { get; set; }

    /// <summary>
    /// List of user IDs to add to the group. Teachers/Admins cannot be added to groups.
    /// </summary>
    public IEnumerable<int>? UserIds { get; set; }
}