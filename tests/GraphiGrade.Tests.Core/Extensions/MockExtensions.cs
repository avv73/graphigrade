using Moq;
using Moq.Protected;
using System.Net;

namespace GraphiGrade.Tests.Core.Extensions;

public static class MockExtensions
{
    public static void SetupHttpResponse(this Mock<IHttpClientFactory> httpClientFactory, string response, HttpStatusCode responseCode)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = responseCode,
                Content = new StringContent(response),
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://example.com/")
        };

        httpClientFactory
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(mockHttpClient);
    }
}