namespace GraphiGrade.Models;

public class ExercisesGroups
{
    public required int ExerciseId { get; set; }
    public required int GroupId { get; set; }

    public required Exercise Exercise { get; set; }
    public required Group Group { get; set; }
}
