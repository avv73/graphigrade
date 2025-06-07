namespace GraphiGrade.Contracts.DTOs.Common;

public record UserGroupDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Uri { get; set; }
}
