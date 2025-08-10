using GraphiGrade.Business.Authorization;
using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.Exercise.Requests;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GraphiGrade.Web.Controllers;

/// <summary>
/// TODO: Create integration for blob storage in the cloud.
/// TODO: Create endpoint for uploading results for judging.
/// TODO: Create endpoint for submission statuses/visualization.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class ExerciseController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IExerciseService _exerciseService;
    private readonly IUserResolverService _userResolverService;

    private readonly IEnumerable<IAuthorizationRequirementErrorProducer> _authRequirements = 
        RequirementsFactory.CreateRequirements(Policy.Admin, Policy.UserHasExercise);

    public ExerciseController(IAuthorizationService authorizationService, IExerciseService exerciseService, IUserResolverService userResolverService)
    {
        _authorizationService = authorizationService;
        _exerciseService = exerciseService;
        _userResolverService = userResolverService;
    }

    [HttpPost]

    [ProducesResponseType<CreateExerciseResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> CreateExerciseAsync(CreateExerciseRequest request, CancellationToken cancellationToken)
    {
        AuthorizationResult authResult = await _authorizationService.AuthorizeAsync(
            User,
            Policy.Admin);


        if (!authResult.Succeeded)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        if (!_userResolverService.Resolve(User))
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        ServiceResult<CreateExerciseResponse> response = await _exerciseService.CreateExerciseAsync(request, cancellationToken);

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
    [Route("{id}")]
    [ProducesResponseType<GetExerciseResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetExerciseByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest))
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        AuthorizationResult authResult = await _authorizationService.AuthorizeAsync(
            User,
            id,
            _authRequirements);

        if (!authResult.Succeeded)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        if (!_userResolverService.Resolve(User))
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        ServiceResult<GetExerciseResponse> response = await _exerciseService.GetExerciseByIdAsync(id, cancellationToken);

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
