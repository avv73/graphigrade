using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using GraphiGrade.Configuration;
using GraphiGrade.Models.Externals.Recaptcha;
using GraphiGrade.Services.Externals.Abstractions;
using GraphiGrade.Services.Identity;
using GraphiGrade.Tests.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GraphiGrade.Tests.UnitTests.Services.Identity;

public class CaptchaValidatorTests
{
    private readonly Mock<ILogger<CaptchaValidator>> _loggerMock;
    private readonly Mock<IRecaptchaApiClient> _recaptchaApiClientMock;

    private readonly CaptchaValidator _captchaValidator;

    private const string TestToken = "token";
    private const string TestUserAgent = "userAgent";
    private const string TestUserIp = "userIp";
    private const string TestUserAction = "userAction";
    private const string TestDifferentUserAction = "differentActoin";

    public CaptchaValidatorTests()
    {
        _loggerMock = new Mock<ILogger<CaptchaValidator>>();
        _recaptchaApiClientMock = new Mock<IRecaptchaApiClient>();

        _captchaValidator =
            new CaptchaValidator(_loggerMock.Object, _recaptchaApiClientMock.Object, MockConfig());
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        ILogger<CaptchaValidator> logger = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CaptchaValidator(logger, _recaptchaApiClientMock.Object, MockConfig()));
    }

    [Fact]
    public void Constructor_NullRecaptchaApiClient_ThrowsArgumentNullException()
    {
        // Arrange
        IRecaptchaApiClient recaptchaApiClient = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CaptchaValidator(_loggerMock.Object, recaptchaApiClient, MockConfig()));
    }

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        IOptions<GraphiGradeConfig> config = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CaptchaValidator(_loggerMock.Object, _recaptchaApiClientMock.Object, config));
    }

    [Fact]
    public void Constructor_ValidParameters_DoesNotThrowException()
    {
        // Arrange
        var config = MockConfig();

        // Act
        var exception = Record.Exception(() =>
            new CaptchaValidator(_loggerMock.Object, _recaptchaApiClientMock.Object, config));

        // Assert
        exception.Should().BeNull();
    }

    #endregion

    #region ValidateCaptchaAsync

    [Fact]
    public async Task
        ValidateCaptchaAsync_WhenRecaptchaResponseIsNull_ShouldReturnInvalidRecaptchaResponseValidationResult()
    {
        // Arrange
        var expectedResponse = new ValidationResult("Received invalid response from ReCAPTCHA API; will not proceed.");

        _recaptchaApiClientMock
            .Setup(x => x.AssessRecaptchaAsync(It.IsAny<RecaptchaRequestDto>()))
            .ReturnsAsync((RecaptchaResponseDto)null);

        // Act
        var result = await _captchaValidator.ValidateCaptchaAsync(TestToken, TestUserAgent, TestUserIp, TestUserAction);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ValidateCaptchaAsync_WhenRecaptchaTokenIsInvalid_ShouldReturnInvalidCaptchaTokenValidationResult()
    {
        // Arrange
        var expectedResponse =
            new ValidationResult("Invalid captcha token passed!");

        var recaptchaResponse = new RecaptchaResponseDto
        {
            TokenProperties = new RecaptchaResponseDtoTokenProperties
            {
                Valid = false
            }
        };

        _recaptchaApiClientMock
            .Setup(x => x.AssessRecaptchaAsync(It.IsAny<RecaptchaRequestDto>()))
            .ReturnsAsync(recaptchaResponse);

        // Act
        var result = await _captchaValidator.ValidateCaptchaAsync(TestToken, TestUserAgent, TestUserIp, TestUserAction);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ValidateCaptchaAsync_WhenUserActionMismatch_ShouldReturnForgedCaptchaTokenValidationResult()
    {
        // Arrange
        var expectedResponse = new ValidationResult("ExpectedAction mismatched in the token passed - forgery attempt!");

        var recaptchaResponse = new RecaptchaResponseDto
        {
            TokenProperties = new RecaptchaResponseDtoTokenProperties
            {
                Valid = true,
                Action = TestDifferentUserAction
            }
        };

        _recaptchaApiClientMock
            .Setup(x => x.AssessRecaptchaAsync(It.IsAny<RecaptchaRequestDto>()))
            .ReturnsAsync(recaptchaResponse);

        // Act
        var result = await _captchaValidator.ValidateCaptchaAsync(TestToken, TestUserAgent, TestUserIp, TestUserAction);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    } 

    #endregion

    private static IOptions<GraphiGradeConfig> MockConfig()
    {
        return Options.Create(GraphiGradeConfigBuilder.BuildConfig());
    }
}
