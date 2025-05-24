using System.Diagnostics;
using System.Text;
using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Judge.Services;

public class JudgeCompilerService : IJudgeCompilerService
{
    // 0 - output location, 1 - cppFileLocation, 2 - includePath, 3 - libPath
    private const string CompilerCommand =
        "g++ -o {0} {1} -I {2} -L {3} -l glew32 -l freeglut -l glfw3dll -l opengl32 -l glu32";
    
    private readonly JudgeRunnerSettings _settings;
    
    public JudgeCompilerService(IOptions<JudgeRunnerSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> CompileAsync(string cppFileLocation, string outputLocation, CancellationToken cancellationToken)
    {
        string compilationCommand = string
            .Format(CompilerCommand, outputLocation, cppFileLocation, _settings.IncludePath, _settings.LibPath)
            .Replace("\\", "/");

        StringBuilder compilationSb = new StringBuilder();

        ProcessStartInfo compilerProcessInfo = new()
        {
            FileName = $"{_settings.CompilerPath}\\w64devkit.exe",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process compilerProcess = new()
        {
            StartInfo = compilerProcessInfo
        };

        compilerProcess.OutputDataReceived += (s, e) => CompilerProcess_OutputDataReceived(s, e, compilationSb);
        compilerProcess.ErrorDataReceived += (s, e) => CompilerProcess_OutputDataReceived(s, e, compilationSb);

        compilerProcess.Start();
        compilerProcess.BeginOutputReadLine();
        compilerProcess.BeginErrorReadLine();

        await compilerProcess.StandardInput.WriteLineAsync(compilationCommand);
        await compilerProcess.StandardInput.FlushAsync();

        // This is a long running process so we need to Sleep and check the output without ReadToEnd()
        await Task.Delay(_settings.MaximumCompileTimeMilliseconds, cancellationToken);
        
        compilerProcess.Kill();

        return compilationSb.ToString();
    }

    private void CompilerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e, StringBuilder sb)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            sb.AppendLine(e.Data);
        }
    }
}
