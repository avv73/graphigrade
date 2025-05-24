using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Services.Identity.Abstractions;

public interface ICaptchaValidator
{
    public Task<ValidationResult?> ValidateCaptchaAsync(string token, string userAgent, string userIp, string userAction);
}