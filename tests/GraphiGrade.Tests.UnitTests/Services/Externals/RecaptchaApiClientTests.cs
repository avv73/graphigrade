using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using GraphiGrade.Configuration;
using GraphiGrade.Models.Externals.Recaptcha;
using GraphiGrade.Services.Externals;
using GraphiGrade.Tests.Core;
using GraphiGrade.Tests.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GraphiGrade.Tests.UnitTests.Services.Externals;
public class RecaptchaApiClientTests
{
    private readonly RecaptchaApiClient _recaptchaApiClient;

    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

    private readonly Mock<ILogger<RecaptchaApiClient>> _loggerMock;

    public RecaptchaApiClientTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<RecaptchaApiClient>>();
        _recaptchaApiClient = new RecaptchaApiClient(MockConfig(), _loggerMock.Object, _httpClientFactoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullConfig_ThrowsNullReferenceException()
    {
        // Arrange
        IOptions<GraphiGradeConfig> config = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RecaptchaApiClient(config, _loggerMock.Object, _httpClientFactoryMock.Object));
    }

    [Fact]
    public void Constructor_NullHttpClientFactory_ThrowsNullReferenceException()
    {
        // Arrange
        IHttpClientFactory httpClientFactory = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RecaptchaApiClient(MockConfig(), _loggerMock.Object, httpClientFactory));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsNullReferenceException()
    {
        // Arrange
        ILogger<RecaptchaApiClient> logger = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RecaptchaApiClient(MockConfig(), logger, _httpClientFactoryMock.Object));
    }

    [Fact]
    public void Constructor_ValidParameters_DoesNotThrowException()
    {
        // Arrange
        var config = MockConfig();

        // Act 
        var exception = Record.Exception(() =>
            new RecaptchaApiClient(config, _loggerMock.Object, _httpClientFactoryMock.Object));

        // Assert
        exception.Should().BeNull();
    }

    #endregion

    #region AssessRecaptchaAsync
    [Fact]
    public async Task AssessRecaptchaAsync_SuccessfulResponse_ReturnsExpectedDto()
    {
        // Arrange
        var request = GenerateSampleRequest();

        var expectedResponseJson = GenerateSampleResponseJson();
        var expectedResponse = JsonSerializer.Deserialize<RecaptchaResponseDto>(expectedResponseJson);

        _httpClientFactoryMock.SetupHttpResponse(expectedResponseJson, HttpStatusCode.OK);

        // Act
        var result = await _recaptchaApiClient.AssessRecaptchaAsync(request);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task AssessRecaptchaAsync_UnsuccessfulResponse_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var request = GenerateSampleRequest();

        const string expectedResponseJson = "{\"message\": \"Unknown error!\"}";

        _httpClientFactoryMock.SetupHttpResponse(expectedResponseJson, statusCode);

        // Act
        var result = await _recaptchaApiClient.AssessRecaptchaAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("An unknown error occurred.")]
    public async Task AssessRecaptchaAsync_InvalidJsonResponse_ReturnsNull(string jsonResponse)
    {
        // Arrange
        var request = GenerateSampleRequest();

        _httpClientFactoryMock.SetupHttpResponse(jsonResponse, HttpStatusCode.OK);

        // Act
        var result = await _recaptchaApiClient.AssessRecaptchaAsync(request);

        // Assert
        result.Should().BeNull();
    } 

    #endregion

    private static RecaptchaRequestDto GenerateSampleRequest()
    {
        return new RecaptchaRequestDto
        {
            EventData = new RecaptchaDtoEvent
            {
                ExpectedAction = "test",
                JaFingerprint = "test",
                SiteKey = "test",
                Token = "test",
                UserAgent = "test",
                UserIpAddress = "1.1.1.1"
            }
        };
    }

    private static string GenerateSampleResponseJson()
    {
        var response = new RecaptchaResponseDto
        {
            EventData = new RecaptchaDtoEvent
            {
                ExpectedAction = "test",
                JaFingerprint = "test",
                SiteKey = "test",
                Token = "test",
                UserAgent = "test",
                UserIpAddress = "1.1.1.1"
            },
            Name = "test",
            RiskAnalysis = new RecaptchaResponseDtoRiskAnalysis
            {
                Score = 0.0f,
                Reasons = ["test"]
            },
            TokenProperties = new RecaptchaResponseDtoTokenProperties
            {
                Action = "test",
                CreateTime = DateTime.UtcNow,
                Hostname = "localhost",
                Valid = true
            }
        };

        return JsonSerializer.Serialize(response);
    }

    private static IOptions<GraphiGradeConfig> MockConfig()
    {
        return Options.Create(GraphiGradeConfigBuilder.BuildConfig());
    }
}