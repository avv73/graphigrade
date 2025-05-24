using GraphiGrade.Judge.Models;

namespace GraphiGrade.Judge.Services.Abstractions;

public interface IJudgeExecutorService
{
    Task<JudgeExecutorResult> ExecuteAsync(string outputProgramPath);
}
