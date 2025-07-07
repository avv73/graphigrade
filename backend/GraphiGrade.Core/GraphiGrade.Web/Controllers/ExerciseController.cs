using GraphiGrade.Business.ServiceModels.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using GraphiGrade.Business.Authorization;
using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Contracts.DTOs;

namespace GraphiGrade.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExerciseController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IExerciseService _exerciseService;

    private readonly IEnumerable<IAuthorizationRequirementErrorProducer> _authRequirements = 
        RequirementsFactory.CreateRequirements(Policy.Admin, Policy.UserHasExercise);

    public ExerciseController(IAuthorizationService authorizationService, IExerciseService exerciseService)
    {
        _authorizationService = authorizationService;
        _exerciseService = exerciseService;
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

        ServiceResult<GetExerciseResponse> response =
            await _exerciseService.GetExerciseByIdAsync(id, User, cancellationToken);

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
