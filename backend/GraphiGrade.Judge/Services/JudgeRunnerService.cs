using GraphiGrade.Judge.Common;
using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.DTOs.Enums;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Services.Abstractions;
using GraphiGrade.Judge.Services.OverlapStrategies.Abstractions;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Judge.Services;

public class JudgeRunnerService : IJudgeRunnerService
{
    private readonly IJudgeCompilerService _compilerService; 
    private readonly IJudgeSourceCodeAnalyzerService _sourceCodeAnalyzer;
    private readonly IJudgeExecutorService _executorService;
    private readonly IJudgeOverlapStrategy _overlapStrategy;
    private readonly ILogger<JudgeRunnerService> _logger;

    private readonly JudgeRunnerSettings _settings;
    
    public JudgeRunnerService(
        IJudgeSourceCodeAnalyzerService sourceCodeAnalyzer,
        IJudgeCompilerService compilerService,
        IOptions<JudgeRunnerSettings> settings,
        IJudgeExecutorService executorService,
        IJudgeOverlapStrategy overlapStrategy,
        ILogger<JudgeRunnerService> logger)
    {
        _settings = settings.Value;
        _sourceCodeAnalyzer = sourceCodeAnalyzer;
        _compilerService = compilerService;
        _executorService = executorService;
        _overlapStrategy = overlapStrategy;
        _logger = logger;
    }

    public async Task<Submission> RunAsync(Submission submission, CancellationToken cancellationToken)
    {
        try
        {
            using (JudgeTempSolutionContainer tempContainer = new(Path.Join(_settings.TempDirPath, submission.Id)))
            {
                _logger.LogInformation($"Starting judging for {tempContainer.SolutionCppFullPath}");

                // Decode source code in memory
                byte[] sourceCodeBytes = Convert.FromBase64String(submission.SourceCodeBase64);
                string sourceCode = System.Text.Encoding.UTF8.GetString(sourceCodeBytes);

                // 1. Perform static analysis
                if (_sourceCodeAnalyzer.IsSourceCodeSuspicious(sourceCode, out var suspicionReasons))
                {
                    _logger.LogWarning(
                        $"Rejected suspicious source code {tempContainer.SolutionCppFullPath} Reasons: {string.Join(Environment.NewLine, suspicionReasons)}");

                    SetSubmissionState(
                        submission,
                        SubmissionErrorCode.FlaggedAsSuspicious,
                        SubmissionStatus.Finished);

                    return submission;
                }

                // Put source code of solution inside the temp folder
                await File.WriteAllBytesAsync(tempContainer.SolutionCppFullPath, sourceCodeBytes, cancellationToken);

                // 2. Compile
                string compilerOutput = await _compilerService.CompileAsync(
                    tempContainer.SolutionCppFullPath,
                    tempContainer.SolutionOutputProgramFullPath, 
                    cancellationToken);

                if (!File.Exists(tempContainer.SolutionOutputProgramFullPath))
                {
                    _logger.LogInformation(
                        $"Failed to compile {tempContainer.SolutionCppFullPath}, Compile Log: {compilerOutput}");

                    SetSubmissionState(
                        submission,
                        SubmissionErrorCode.CompilationFailed,
                        SubmissionStatus.Finished,
                        compilerOutput);

                    return submission;
                }

                // 3. Execute
                JudgeExecutorResult executionResult = await _executorService.ExecuteAsync(tempContainer.SolutionOutputProgramFullPath);

                if (!executionResult.IsSuccessful)
                {
                    _logger.LogError(
                        $"Failed to execute or capture: {tempContainer.SolutionOutputProgramFullPath}, Error: {executionResult.ErrorMessage}");

                    SetSubmissionState(
                        submission,
                        SubmissionErrorCode.CompilationFailed,
                        SubmissionStatus.Finished,
                        executionResult.ErrorMessage!);

                    return submission;
                }

                // Decode expected result image
                ImageDecorator expectedResult = ImageDecorator.FromBase64(submission.ExpectedPatternBase64);

                // 4. Judge using the strategy
                double accuracyScore = _overlapStrategy.OverlapSolutionWithExpectedResult(
                    executionResult.ExecutionResult!,
                    expectedResult,
                    out ImageDecorator? overlappedResult);

                SetSubmissionState(
                    submission,
                    SubmissionErrorCode.None,
                    SubmissionStatus.Finished,
                    errorDetails: string.Empty,
                    accuracyScore,
                    overlappedResult?.ToBase64() ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Unhandled exception while judging submission!");

            SetSubmissionState(
                submission,
                SubmissionErrorCode.UnknownExecutionError,
                SubmissionStatus.Finished,
                ex.Message);
        }

        return submission;
    }

    private void SetSubmissionState(
        Submission submission, 
        SubmissionErrorCode errorCode,
        SubmissionStatus status, 
        string errorDetails = "", 
        double resultAccuracy = 0, 
        string resultPatternBase64 = "")
    {
        submission.ErrorCode = errorCode;
        submission.Status = status;
        submission.LastUpdated = DateTime.Now;
        submission.ErrorDetails = errorDetails;
        submission.ExecutionResultBase64 = resultPatternBase64;
        submission.ExecutionAccuracy = resultAccuracy;
    }

    private class JudgeTempSolutionContainer : IDisposable
    {
        public string SolutionFolderPath { get; }

        public static string SolutionCppName => "Program.cpp";

        public static string OutputProgramName => "Program.exe";

        public string SolutionCppFullPath { get; }

        public string SolutionOutputProgramFullPath { get; set; }

        public JudgeTempSolutionContainer(string solutionFolderPath)
        {
            SolutionFolderPath = solutionFolderPath;

            if (Directory.Exists(solutionFolderPath))
            {
                throw new Exception("Temp folder already created!");
            }

            Directory.CreateDirectory(SolutionFolderPath);

            SolutionCppFullPath = Path.Join(SolutionFolderPath, SolutionCppName);
            SolutionOutputProgramFullPath = Path.Join(SolutionFolderPath, OutputProgramName);
        }

        public void Dispose()
        {
            // Maybe schedule for deletion?
            //Directory.Delete(SolutionFolderPath, true);
        }
    }
}
