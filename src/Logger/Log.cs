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

    // 2000 - Warning

    [LoggerMessage(
        Level = LogLevel.Warning,
        EventId = 2000,
        Message = "{Timestamp} | {ServiceName} | {MethodName} | {Message}")]
    public static partial void LogGeneralWarning(
        this ILogger logger,
        DateTime timestamp,
        string serviceName,
        string methodName,
        string message);

    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 3001,
        Message = "{Timestamp} | {ServiceName} | {MethodName} | {Request} | {Response}")]
    public static partial void LogExternalApiError(
        this ILogger logger,
        DateTime timestamp,
        string serviceName,
        string methodName,
        string request,
        string response);

    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 3002,
        Message = "{Timestamp} | {ServiceName} | {MethodName} | {Message}")]
    public static partial void LogServiceException(
        this ILogger logger,
        DateTime timestamp,
        string serviceName,
        string methodName,
        string message,
        Exception ex);

    // 4000 - Critical

    // 5000 - Debug
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