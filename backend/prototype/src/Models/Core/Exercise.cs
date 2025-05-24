using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Models.Core;

public class Exercise
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(10000)]
    public string? Description { get; set; }


}
