namespace GraphiGrade.Judge.Services.Abstractions;

public interface IJudgeCompilerService
{
    Task<string> CompileAsync(string cppFileLocation, string outputLocation, CancellationToken cancellationToken);
}
