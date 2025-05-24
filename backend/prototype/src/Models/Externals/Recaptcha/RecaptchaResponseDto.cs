using System.Text.Json.Serialization;

namespace GraphiGrade.Models.Externals.Recaptcha;

public class RecaptchaResponseDto
{
    [JsonPropertyName("tokenProperties")]
    public RecaptchaResponseDtoTokenProperties TokenProperties { get; set; } = null!;

    [JsonPropertyName("riskAnalysis")]
    public RecaptchaResponseDtoRiskAnalysis RiskAnalysis { get; set; } = null!;

    [JsonPropertyName("event")]
    public RecaptchaDtoEvent EventData { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}

public class RecaptchaResponseDtoTokenProperties
{
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("hostname")]
    public string Hostname { get; set; } = null!;

    [JsonPropertyName("action")]
    public string Action { get; set; } = null!;

    [JsonPropertyName("createTime")]
    public DateTime CreateTime { get; set; }
}

public class RecaptchaResponseDtoRiskAnalysis
{
    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("reasons")]
    public string[] Reasons { get; set; } = null!;
}