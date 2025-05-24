using GraphiGrade.Configuration;
using GraphiGrade.Services.Externals;
using GraphiGrade.Tests.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using FluentAssertions;
using GraphiGrade.Tests.Core.Extensions;

namespace GraphiGrade.Tests.UnitTests.Services.Externals;

public class MailgunApiClientTests
{
    private readonly MailgunApiClient _mailgunApiClient;

    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

    private readonly Mock<ILogger<MailgunApiClient>> _loggerMock;

    private const string SenderEmailTest = "mail@abv.bg";

    private const string RecipientEmailTest = "mail@gmail.com";

    private const string SubjectTest = "Test Subject";

    private const string TextContentTest = "Text Content";

    private const string HtmlContentTest = "Html Content";

    public MailgunApiClientTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<MailgunApiClient>>();
        _mailgunApiClient = new MailgunApiClient(MockConfig(), _httpClientFactoryMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullConfig_ThrowsNullReferenceException()
    {
        // Arrange
        IOptions<GraphiGradeConfig> config = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MailgunApiClient(config, _httpClientFactoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullHttpClientFactory_ThrowsNullReferenceException()
    {
        // Arrange
        IHttpClientFactory httpClientFactory = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MailgunApiClient(MockConfig(), httpClientFactory, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsNullReferenceException()
    {
        // Arrange
        ILogger<MailgunApiClient> logger = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MailgunApiClient(MockConfig(), _httpClientFactoryMock.Object, logger));
    }

    [Fact]
    public void Constructor_ValidParameters_DoesNotThrowException()
    {
        // Arrange
        var config = MockConfig();

        // Act 
        var exception = Record.Exception(() =>
            new MailgunApiClient(config, _httpClientFactoryMock.Object, _loggerMock.Object));

        // Assert
        exception.Should().BeNull();
    }

    #endregion

    #region SendMailAsync
    [Fact]
    public async Task SendMailAsync_SuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        _httpClientFactoryMock.SetupHttpResponse("{\"id\": \"100\", \"status\": \"Successful\"}", HttpStatusCode.OK);

        // Act
        bool result =
            await _mailgunApiClient.SendMailAsync(SenderEmailTest, RecipientEmailTest, SubjectTest, TextContentTest, HtmlContentTest);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task SendMailAsync_UnsuccessfulResponse_ReturnsFalse(HttpStatusCode statusCode)
    {
        // Arrange
        _httpClientFactoryMock.SetupHttpResponse("Error occurred", statusCode);

        // Act
        bool result =
            await _mailgunApiClient.SendMailAsync(SenderEmailTest, RecipientEmailTest, SubjectTest, TextContentTest, HtmlContentTest);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    private static IOptions<GraphiGradeConfig> MockConfig()
    {
        return Options.Create(GraphiGradeConfigBuilder.BuildConfig());
    }
}