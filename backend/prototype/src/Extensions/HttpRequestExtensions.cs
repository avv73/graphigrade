using System.Net;

namespace GraphiGrade.Extensions;

public static class HttpRequestExtensions
{
    public static string? GetUserAgent(this HttpRequest request)
    {
        return request.Headers["User-Agent"];
    }

    public static string? GetUserIp(this HttpRequest request)
    {
        string? userIp = request.Headers["X-Forwarded-For"];
        if (string.IsNullOrWhiteSpace(userIp))
        {
            userIp = request.HttpContext.Connection.RemoteIpAddress?.ToString(); 
        }
        return userIp;
    }
}