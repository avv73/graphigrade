namespace GraphiGrade.Judge.Services.Abstractions;

public interface IJudgeSourceCodeAnalyzerService
{
    bool IsSourceCodeSuspicious(string sourceCode, out IEnumerable<string> suspicionReasons);
}
