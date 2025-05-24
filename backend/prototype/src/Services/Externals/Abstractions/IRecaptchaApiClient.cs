using GraphiGrade.Models.Externals.Recaptcha;

namespace GraphiGrade.Services.Externals.Abstractions;

public interface IRecaptchaApiClient
{
    Task<RecaptchaResponseDto?> AssessRecaptchaAsync(RecaptchaRequestDto  recaptchaRequest);
}