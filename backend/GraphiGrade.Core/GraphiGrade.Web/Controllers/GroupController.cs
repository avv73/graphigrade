using System.Net;
using GraphiGrade.Business.Authorization;
using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs;
using GraphiGrade.Contracts.DTOs.Common;
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

    public GroupController(IAuthorizationService authorizationService, IGroupService groupService)
    {
        _authorizationService = authorizationService;
        _groupService = groupService;
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

        // Check if user is Admin OR belongs to the group
        var adminCheck = await _authorizationService.AuthorizeAsync(User, Policy.Admin);
        var memberCheck = await _authorizationService.AuthorizeAsync(User, id, Policy.UserBelongsToGroup);

        if (!adminCheck.Succeeded && !memberCheck.Succeeded)
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

    [HttpGet]
    [Route("all")]
    [ProducesResponseType(typeof(GetAllGroupsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetAllGroupsAsync(CancellationToken cancellationToken)
    {
        // Only admins can view all groups
        var adminCheck = await _authorizationService.AuthorizeAsync(User, Policy.Admin);
        if (!adminCheck.Succeeded)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        var response = await _groupService.GetAllGroupsAsync(cancellationToken);
        if (response.IsError)
        {
            return new ObjectResult(response.Error)
            {
                StatusCode = (int)response.Error!.ErrorCode
            };
        }

        return Ok(response.Result);
    }

    [HttpPut]
    [Route("{id}/assign/{student_id}")]
    [ProducesResponseType(typeof(UserAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignStudentToGroupAsync(int id, int student_id, CancellationToken cancellationToken)
    {
        if (id <= 0 || student_id <= 0)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.BadRequest, "Invalid identifiers"))
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
        var adminCheck = await _authorizationService.AuthorizeAsync(User, Policy.Admin);
        if (!adminCheck.Succeeded)
        {
            return new ObjectResult(ErrorResponseFactory.CreateError(HttpStatusCode.Forbidden))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        var result = await _groupService.AssignStudentToGroupAsync(id, student_id, cancellationToken);
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
