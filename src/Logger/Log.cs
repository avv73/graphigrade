namespace GraphiGrade.Logger;

public static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 1000,
        Message = "{Timestamp} | {ServiceName} | {MethodName} | {Message}")]
    public static partial void LogGeneralInformation(
        this ILogger logger,
        DateTime timestamp,
        string serviceName,
        string methodName,
        string message);

    // 200 - Warning

    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 3001,
        Message = "{Timestamp} | {ServiceName} | {MethodName} | Request: {Request} | Response: {Response}")]
    public static partial void LogExternalApiError(
        this ILogger logger,
        DateTime timestamp,
        string serviceName,
        string methodName,
        string request,
        string response);

    // 400 - Critical

    // 500 - Debug
    [LoggerMessage(
        Level = LogLevel.Debug,
        EventId = 5000,
        Message = "{Timestamp} | {ServiceName} | {MethodName} | {Message}")]
    public static partial void LogGeneralDebug(
        this ILogger logger,
        DateTime timestamp,
        string serviceName,
        string methodName,
        string message);
}