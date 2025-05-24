namespace GraphiGrade.Judge.Configuration;

public class JudgeRunnerSettings
{
    public int MaximumCompileTimeMilliseconds { get; set; }

    public int MaximumRunningTimeMilliseconds { get; set; }

    public int MaximumRunnerThreads { get; set; }

    public long MaximumCharsInSubmissionSourceCode { get; set; }

    public long MaximumBytesSizeOfResultPattern { get; set; }

    public int UnprocessedSubmissionChannelMaximumMessages { get; set; }

    public int ProcessedSubmissionChannelMaximumMessages { get; set; }

    public int RefreshIntervalMilliseconds { get; set; }

    public string CompilerPath { get; set; }

    public string DllFolderPath { get; set; }

    public string IncludePath { get; set; }

    public string LibPath { get; set; }

    public string TempDirPath { get; set; }

    public long MaximumBase64CharsInSubmissionCode => (long)Math.Ceiling(MaximumCharsInSubmissionSourceCode / 3.0) * 4;

    public long MaximumBase64CharsInResultPattern => (long)Math.Ceiling(MaximumBytesSizeOfResultPattern / 3.0) * 4;
}
