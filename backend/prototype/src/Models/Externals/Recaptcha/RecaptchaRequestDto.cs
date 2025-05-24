using System.Text.Json.Serialization;

namespace GraphiGrade.Models.Externals.Recaptcha;

public class RecaptchaRequestDto
{
    [JsonPropertyName("event")]
    public RecaptchaDtoEvent EventData { get; set; } = null!;
}

public class RecaptchaDtoEvent
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;

    [JsonPropertyName("siteKey")]
    public string SiteKey { get; set; } = null!;

    [JsonPropertyName("userAgent")]
    public string UserAgent { get; set; } = null!;

    [JsonPropertyName("userIpAddress")]
    public string UserIpAddress { get; set; } = null!;

    [JsonPropertyName("ja3")]
    public string JaFingerprint { get; set; } = null!;

    [JsonPropertyName("expectedAction")]
    public string ExpectedAction { get; set; } = null!;
}