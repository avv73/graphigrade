namespace GraphiGrade.Models;

public class ExercisesGroups
{
    public int ExerciseId { get; set; }
    public int GroupId { get; set; }

    public Exercise Exercise { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
