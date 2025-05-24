using GraphiGrade.Judge.Common;

namespace GraphiGrade.Judge.Models;

public class JudgeExecutorResult
{
    public bool IsSuccessful { get; set; }

    public string? ErrorMessage { get; set; }

    public ImageDecorator? ExecutionResult { get; set; }
}
