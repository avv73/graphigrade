using System.Net;
using GraphiGrade.Business.Authorization;
using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.Group.Requests;
using GraphiGrade.Contracts.DTOs.Group.Responses;
using GraphiGrade.Contracts.DTOs.User.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraphiGrade.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IGroupService _groupService;

    private readonly IEnumerable<IAuthorizationRequirementErrorProducer> _authRequirements;

    public GroupController(IAuthorizationService authorizationService, IGroupService groupService)
    {
        _authorizationService = authorizationService;
        _groupService = groupService;

        _authRequirements = RequirementsFactory.CreateRequirements(Policy.Admin, Policy.UserBelongsToGroup);
    }

    [HttpPost]
    [ProducesResponseType<CreateGroupResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken)
    {
        // Validate model state
        if (!ModelState.IsValid)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest, "Invalid request data"))
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        // Check if user is authorized (teacher/admin)
        AuthorizationResult authResult = await _authorizationService.AuthorizeAsync(
            User,
            Policy.Admin);

        if (!authResult.Succeeded)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden, "You must be a teacher to create groups"))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        ServiceResult<CreateGroupResponse> response = await _groupService.CreateGroupAsync(request, cancellationToken);

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
    [ProducesResponseType<GetGroupResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetGroupByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest, "Invalid group ID"))
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

        ServiceResult<GetGroupResponse> response = await _groupService.GetGroupByIdAsync(id, cancellationToken);

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
