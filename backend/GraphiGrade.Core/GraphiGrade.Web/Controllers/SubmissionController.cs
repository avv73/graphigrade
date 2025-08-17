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
    private readonly IAuthorizationService _authorizationService;

    public SubmissionController(
        ISubmissionService submissionService,
        IUserResolverService userResolverService,
        IAuthorizationService authorizationService)
    {
        _submissionService = submissionService;
        _userResolverService = userResolverService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType<GetSubmissionStatusResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetSubmissionStatusAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest, "Submission ID is invalid"))
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

        ServiceResult<GetSubmissionStatusResponse> response = await _submissionService.GetSubmissionStatusAsync(id, cancellationToken);

        if (response.IsError)
        {
            return new ObjectResult(response.Error)
            {
                StatusCode = (int)response.Error!.ErrorCode
            };
        }

        return Ok(response.Result);
    }

    [HttpGet]
    [Route("user/{username}")]
    [ProducesResponseType<GetUserSubmissionsResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetUserSubmissionsAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest, "Invalid username"))
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        // Admin or same user only
        var adminCheck = await _authorizationService.AuthorizeAsync(User, Business.Authorization.Policy.Admin);
        var sameUserCheck = await _authorizationService.AuthorizeAsync(User, username, Business.Authorization.Policy.SameUser);

        if (!adminCheck.Succeeded && !sameUserCheck.Succeeded)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        var result = await _submissionService.GetUserSubmissionsAsync(username, cancellationToken);
        if (result.IsError)
        {
            return new ObjectResult(result.Error)
            {
                StatusCode = (int)result.Error!.ErrorCode
            };
        }

        return Ok(result.Result);
    }
}