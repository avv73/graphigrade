using GraphiGrade.Business.Configurations;
using GraphiGrade.Business.ServiceModels.Judge;
using GraphiGrade.Business.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GraphiGrade.Business.Services;

public class JudgeService : IJudgeService
{
    private readonly HttpClient _httpClient;
    private readonly GraphiGradeConfig _config;
    private readonly ILogger<JudgeService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public JudgeService(HttpClient httpClient, IOptions<GraphiGradeConfig> config, ILogger<JudgeService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<JudgeBatchResponse?> SubmitForJudgingAsync(JudgeBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_config.JudgeServiceUrl}/submissions/batch", content, cancellationToken);
            
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<JudgeBatchResponse>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with judge service");
            return null;
        }
    }
}