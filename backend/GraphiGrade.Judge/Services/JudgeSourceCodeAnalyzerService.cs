using System.Text.RegularExpressions;
using GraphiGrade.Judge.Services.Abstractions;

namespace GraphiGrade.Judge.Services;

public partial class JudgeSourceCodeAnalyzerService : IJudgeSourceCodeAnalyzerService
{
    private static string[] FileHeaders = new[]
    {
        "#include <fstream>", "#include <cstdio>", "#include <stdio.h>", "#include <windows.h>", "#include <filesystem>", "#include <direct.h>"
    };

    private static string[] NetworkHeaders = new[]
    {
        "#include <WinSock2.h>", "#include <winsock.h>", "#include <curl/curl.h>", "#include <wininet.h>", "#include <winhttp.h>"
    };

    private static string[] FileFunctions = new[]
    {
        "remove", "unlink", "DeleteFile", "fopen", "fwrite", "fclose", "ofstream", "ifstream", "CreateFile"
    };

    private static string[] NetworkFunctions = new[]
    {
        "socket", "connect", "send", "recv", "closesocket",
        "InternetOpen", "InternetConnect", "HttpSendRequest",
        "WinHttpOpen", "WinHttpConnect", "WinHttpSendRequest",
        "curl_easy_init", "curl_easy_setopt", "curl_easy_perform"
    };

    public bool IsSourceCodeSuspicious(string sourceCode, out IEnumerable<string> suspicionReasons)
    {
        bool isSuspicious = false;
        List<string> reasons = new List<string>();

        // Check for I/O headers
        isSuspicious = PerformStaticAnalysis(sourceCode, reasons, nameof(FileHeaders), FileHeaders);

        // Check for network headers
        isSuspicious = PerformStaticAnalysis(sourceCode, reasons, nameof(NetworkHeaders), NetworkHeaders);

        // Check for file functions
        isSuspicious = PerformStaticAnalysis(sourceCode, reasons, nameof(FileFunctions), FileFunctions);

        // Check for network functions
        isSuspicious = PerformStaticAnalysis(sourceCode, reasons, nameof(NetworkFunctions), NetworkFunctions);

        // Check for inline assembly code
        if (AssemblyRegex().IsMatch(sourceCode))
        {
            isSuspicious = true;
            Match match = AssemblyRegex().Match(sourceCode);

            reasons.Add($"[Inline Assembly] {match.Value}");
        }

        // Check for suspicious macros
        if (MacrosRegex().IsMatch(sourceCode))
        {
            isSuspicious = true;
            Match match = MacrosRegex().Match(sourceCode);

            reasons.Add($"[Suspicious Macro] {match.Value}");
        }

        // Check for manual syscalls
        if (SyscallsRegex().IsMatch(sourceCode))
        {
            isSuspicious = true;
            Match match = SyscallsRegex().Match(sourceCode);

            reasons.Add($"[Manual Syscall] {match.Value}");
        }

        suspicionReasons = reasons;
        return isSuspicious;
    }

    private static bool PerformStaticAnalysis(string sourceCode, List<string> reasons, string typeOfAnalysis, string[] suspicionStrings)
    {
        bool isSuspicious = false;
        foreach (string suspicionString in suspicionStrings)
        {
            if (sourceCode.Contains(suspicionString, StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add($"[{typeOfAnalysis}] {suspicionString}");
                isSuspicious = true;
            }
        }

        return isSuspicious;
    }

    [GeneratedRegex(@"\b(__asm|asm)\b")]
    private static partial Regex AssemblyRegex();

    [GeneratedRegex(@"#define (DeleteFile|socket|Nt[A-Z])\b")]
    private static partial Regex MacrosRegex();

    [GeneratedRegex(@"\b(GetProcAddress|LoadLibrary|VirtualAlloc|Nt[A-Z])\b")]
    private static partial Regex SyscallsRegex();
}
