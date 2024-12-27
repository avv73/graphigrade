using System.Net.Http.Headers;
using System.Text;
using GraphiGrade.Configuration;
using GraphiGrade.Extensions;
using GraphiGrade.Logger;
using GraphiGrade.Services.Externals.Abstractions;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Services.Externals;

public class MailgunApiClient : IMailgunApiClient
{
    private readonly MailgunConfig _mailgunConfig;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger<MailgunApiClient> _logger;

    private const string ServiceName = nameof(MailgunApiClient);

    public MailgunApiClient(
        IOptions<GraphiGradeConfig>? graphiGradeConfig, 
        IHttpClientFactory? httpClientFactory,
        ILogger<MailgunApiClient>? logger)
    {
        ArgumentNullException.ThrowIfNull(_mailgunConfig = graphiGradeConfig?.Value.MailgunConfig!);
        ArgumentNullException.ThrowIfNull(_httpClientFactory = httpClientFactory!);
        ArgumentNullException.ThrowIfNull(_logger = logger!);
    }

    public async Task<bool> SendMailAsync(string senderEmail, string recipientEmail, string subject, string textContent)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        string mailgunBaseUrl = _mailgunConfig.MailgunEndpointUrl;
        string mailgunApiKey = _mailgunConfig.MailgunApiKey;

        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{mailgunApiKey}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        var formData = new MultipartFormDataContent
        {
            { new StringContent(senderEmail), "from" },
            { new StringContent(recipientEmail), "to" },
            { new StringContent(subject), "subject" },
            { new StringContent(textContent), "text" }
        };

        var response = await httpClient.PostAsync(mailgunBaseUrl, formData);

        var responseAsString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogExternalApiError(
                DateTime.UtcNow, 
                ServiceName, 
                nameof(SendMailAsync), 
                string.Join(Environment.NewLine, await formData.ToDictionaryAsync()),
                responseAsString);

            return false;
        }

        _logger.LogGeneralInformation(
            DateTime.UtcNow, 
            ServiceName, 
            nameof(SendMailAsync), 
            $"Successfully sent email from {senderEmail} to {recipientEmail}. Subject: {subject} Content: {textContent}");

        return true;
    }
}
