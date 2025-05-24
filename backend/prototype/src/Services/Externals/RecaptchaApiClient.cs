using GraphiGrade.Configuration;
using GraphiGrade.Logger;
using GraphiGrade.Models.Externals.Recaptcha;
using GraphiGrade.Services.Externals.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GraphiGrade.Services.Externals;

public class RecaptchaApiClient : IRecaptchaApiClient
{
    private readonly RecaptchaConfig _recaptchaConfig;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger<RecaptchaApiClient> _logger;

    private const string ServiceName = nameof(RecaptchaApiClient);

    public RecaptchaApiClient(
        IOptions<GraphiGradeConfig>? config,
        ILogger<RecaptchaApiClient>? logger,
        IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(_recaptchaConfig = config?.Value.RecaptchaConfig!);
        ArgumentNullException.ThrowIfNull(_logger = logger!);
        ArgumentNullException.ThrowIfNull(_httpClientFactory = httpClientFactory);
    }

    public async Task<RecaptchaResponseDto?> AssessRecaptchaAsync(RecaptchaRequestDto recaptchaRequest)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        string endpointUrl = _recaptchaConfig.RecaptchaAssessUrl;
        string apiKey = _recaptchaConfig.GoogleApiKey;

        string responseAsString = string.Empty;
        string requestBody = string.Empty;

        try
        {
            requestBody = JsonSerializer.Serialize(recaptchaRequest);

            var httpRequestContent = new StringContent(requestBody);

            endpointUrl = string.Format(endpointUrl, apiKey);

            var response = await httpClient.PostAsync(endpointUrl, httpRequestContent);

            responseAsString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogExternalApiError(
                    DateTime.UtcNow,
                    ServiceName,
                    nameof(AssessRecaptchaAsync),
                    requestBody,
                    responseAsString);

                return null;
            }

            var parsedResponse = JsonSerializer.Deserialize<RecaptchaResponseDto>(responseAsString);

            _logger.LogGeneralInformation(
                DateTime.UtcNow,
                ServiceName,
                nameof(AssessRecaptchaAsync),
                $"Recaptcha Assessment Successful for Token: {recaptchaRequest.EventData.Token}");

            return parsedResponse;  
        }
        catch (Exception ex)
        {
            _logger.LogServiceException(
                DateTime.UtcNow,
                ServiceName,
                nameof(AssessRecaptchaAsync),
                $"Request: {requestBody} | Response: {responseAsString}",
                ex);

            return null;
        }
    }
}