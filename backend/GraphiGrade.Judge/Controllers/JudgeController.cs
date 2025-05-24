using System.Net;
using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.DTOs.Enums;
using GraphiGrade.Judge.Extensions;
using GraphiGrade.Judge.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GraphiGrade.Judge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JudgeController : ControllerBase
{
    private readonly ISubmissionsBatchService _submissionsBatchService;
    private readonly ILogger<JudgeController> _logger;

    public JudgeController(
        ISubmissionsBatchService submissionsBatchService,
        ILogger<JudgeController> logger)
    {
        _submissionsBatchService = submissionsBatchService;
        _logger = logger;
    }

    [HttpPost("submissions/batch")]
    public async Task<IActionResult> BatchSubmissionAsync([FromBody] JudgeSubmissionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            JudgeBatchResponse response = await _submissionsBatchService.BatchSubmissionAsync(request, cancellationToken);

            Response.StatusCode = (int)response.MapToHttpStatusCode(returnAcceptedInsteadOK: true);

            return new JsonResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Unhandled exception in controller!");

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new JsonResult(new JudgeBatchResponse
            {
                ErrorCode = SubmissionErrorCode.UnknownProcessingError,
                Timestamp = DateTime.Now
            });
        }
    }

    [HttpGet("submissions/{submissionId}")]
    public async Task<IActionResult> GetSubmissionAsync(string submissionId, CancellationToken cancellationToken)
    {
        try
        {
            JudgeBatchResponse response = await _submissionsBatchService.GetSubmissionAsync(submissionId, cancellationToken);

            Response.StatusCode = (int)response.MapToHttpStatusCode();

            return new JsonResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Unhandled exception in controller!");

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new JsonResult(new JudgeBatchResponse
            {
                ErrorCode = SubmissionErrorCode.UnknownExecutionError,
                Timestamp = DateTime.Now
            });
        }
    }
}
