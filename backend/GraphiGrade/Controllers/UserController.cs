using GraphiGrade.Constants;
using GraphiGrade.DTOs;
using GraphiGrade.DTOs.User.Responses;
using GraphiGrade.Models.ServiceModels;
using GraphiGrade.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraphiGrade.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = Policy.SameUserOrAdmin)]
    [Route("{username}")]
    [ProducesResponseType<GetUserResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
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
