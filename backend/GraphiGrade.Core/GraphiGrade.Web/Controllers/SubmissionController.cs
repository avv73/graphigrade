using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.Submission.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GraphiGrade.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class SubmissionController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly IUserResolverService _userResolverService;

    public SubmissionController(
        ISubmissionService submissionService,
        IUserResolverService userResolverService)
    {
        _submissionService = submissionService;
        _userResolverService = userResolverService;
    }

    [HttpGet]
    [Route("{submissionId}")]
    [ProducesResponseType<GetSubmissionStatusResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetSubmissionStatusAsync(string submissionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(submissionId))
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest, "Submission ID cannot be empty"))
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        if (!_userResolverService.Resolve(User))
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        ServiceResult<GetSubmissionStatusResponse> response = await _submissionService.GetSubmissionStatusAsync(submissionId, cancellationToken);

        if (response.IsError)
        {
            return new ObjectResult(response.Error)
            {
                StatusCode = (int)response.Error!.ErrorCode
            };
        }

        return Ok(response.Result);
    }
}