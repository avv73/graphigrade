using GraphiGrade.Configuration;
using GraphiGrade.Logger;
using GraphiGrade.Models.Externals.Recaptcha;
using GraphiGrade.Services.Externals.Abstractions;
using GraphiGrade.Services.Identity.Abstractions;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace GraphiGrade.Services.Identity;

public class CaptchaValidator : ICaptchaValidator
{
    private readonly RecaptchaConfig _config;

    private readonly ILogger<CaptchaValidator> _logger;

    private readonly IRecaptchaApiClient _recaptchaApiClient;

    private static readonly ValidationResult InvalidRecaptchaResponseValidationResult =
        new("Received invalid response from ReCAPTCHA API; will not proceed.");

    private static readonly ValidationResult InvalidCaptchaTokenValidationResult = 
        new("Invalid captcha token passed!");

    private static readonly ValidationResult ForgedCaptchaTokenValidationResult =
        new("ExpectedAction mismatched in the token passed - forgery attempt!");

    private const string ServiceName = nameof(CaptchaValidator);

    public CaptchaValidator(
        ILogger<CaptchaValidator>? logger, 
        IRecaptchaApiClient? recaptchaApiClient,
        IOptions<GraphiGradeConfig>? config)
    {
        ArgumentNullException.ThrowIfNull(_logger = logger!);
        ArgumentNullException.ThrowIfNull(_recaptchaApiClient = recaptchaApiClient!);
        ArgumentNullException.ThrowIfNull(_config = config?.Value.RecaptchaConfig!);
    }


    public async Task<ValidationResult?> ValidateCaptchaAsync(string token, string userAgent, string userIp, string userAction)
    {
        var resultList = new List<ValidationResult>();
        
        var recaptchaRequestDto = new RecaptchaRequestDto
        {
            EventData = new RecaptchaDtoEvent
            {
                Token = token,
                UserAgent = userAgent,
                UserIpAddress = userIp,
                SiteKey = _config.SiteKey,
                ExpectedAction = userAction
            }
        };

        var recaptchaResponse = await _recaptchaApiClient.AssessRecaptchaAsync(recaptchaRequestDto);

        if (recaptchaResponse == null)
        {
            _logger.LogGeneralInformation(
                DateTime.UtcNow, 
                ServiceName, 
                nameof(ValidateCaptchaAsync), 
                $"Invalid captcha API response for token {token}, {userAction}, {userAgent}, {userIp}.");

            return InvalidRecaptchaResponseValidationResult;
        }

        if (!recaptchaResponse.TokenProperties.Valid)
        {
            _logger.LogGeneralInformation(
                DateTime.UtcNow,
                ServiceName,
                nameof(ValidateCaptchaAsync),
                $"Invalid captcha token {token}, {userAction}, {userAgent}, {userIp}.");

            return InvalidCaptchaTokenValidationResult;
        }

        if (!recaptchaResponse.TokenProperties.Action.Equals(userAction, StringComparison.InvariantCulture))
        {
            _logger.LogGeneralWarning(
                DateTime.UtcNow,
                ServiceName,
                nameof(ValidateCaptchaAsync),
                $"Forgery attempt - {token}, {userAction}, {userAgent}, {userIp}.");

            return ForgedCaptchaTokenValidationResult;
        }

        return ValidationResult.Success;
    }
}