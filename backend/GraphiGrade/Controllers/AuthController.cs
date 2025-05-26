using GraphiGrade.DTOs;
using GraphiGrade.DTOs.Auth.Requests;
using GraphiGrade.DTOs.Auth.Responses;
using GraphiGrade.Models.ServiceModels;
using GraphiGrade.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GraphiGrade.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [Route("register")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<RegisterResponse> response = await _userService.RegisterUserAsync(request, cancellationToken);

        if (response.IsError)
        {
            return new ObjectResult(response.Error)
            {
                StatusCode = (int)response.Error!.ErrorCode
            };
        }
        

        return Created($"api/users/{response.Result!.Username}", response);
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<LoginResponse> response = await _userService.LoginUserAsync(request, cancellationToken);

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
