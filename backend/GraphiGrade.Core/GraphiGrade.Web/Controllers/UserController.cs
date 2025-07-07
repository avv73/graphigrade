using System.Net;
using GraphiGrade.Business.Authorization;
using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.User.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraphiGrade.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authorizationService;

    private readonly IEnumerable<IAuthorizationRequirementErrorProducer> _authRequirements;
    
    public UserController(IUserService userService, IAuthorizationService authorizationService)
    {
        _userService = userService;
        _authorizationService = authorizationService;

        _authRequirements = RequirementsFactory.CreateRequirements(Policy.Admin, Policy.SameUser);
    }

    [HttpGet]
    [Route("{username}")]
    [ProducesResponseType<GetUserResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest))
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        AuthorizationResult authResult = await _authorizationService.AuthorizeAsync(
            User, 
            username, 
            _authRequirements);

        if (!authResult.Succeeded)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        ServiceResult<GetUserResponse> response = await _userService.GetUserByUsernameAsync(username, cancellationToken);

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
