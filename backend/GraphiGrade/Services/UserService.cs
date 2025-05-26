using System.Diagnostics;
using GraphiGrade.DTOs.Auth.Responses;
using GraphiGrade.DTOs.User.Responses;
using GraphiGrade.Models;
using GraphiGrade.Models.ServiceModels;
using GraphiGrade.Repositories.Abstractions;
using GraphiGrade.Services.Abstractions;
using GraphiGrade.Services.Utils.Abstractions;
using System.Net;
using GraphiGrade.Mappers.Abstractions;
using RegisterRequest = GraphiGrade.DTOs.Auth.Requests.RegisterRequest;
using LoginRequest = GraphiGrade.DTOs.Auth.Requests.LoginRequest;

namespace GraphiGrade.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUserMapper _userMapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtService jwtService, IUserMapper userMapper, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _userMapper = userMapper;
        _logger = logger;
    }

    public async Task<ServiceResult<RegisterResponse>> RegisterUserAsync(RegisterRequest registerRequest, CancellationToken cancellationToken)
    {
        bool isUserWithSameName;

        try
        {
            isUserWithSameName = await _unitOfWork.Users.ExistsAsync(u => u.Username == registerRequest.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when checking existing user!");

            return ServiceResultFactory<RegisterResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when registering, please try again later");
        }

        if (isUserWithSameName)
        {
            return ServiceResultFactory<RegisterResponse>.CreateError(HttpStatusCode.BadRequest, "Username already taken");
        }

        string hashedPassword = _passwordHasher.HashPassword(registerRequest.Password);

        User newUser = new User
        {
            Username = registerRequest.Username,
            Password = hashedPassword
        };

        try
        {
            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when registering user!");

            return ServiceResultFactory<RegisterResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when registering, please try again later");
        }

        return ServiceResultFactory<RegisterResponse>.CreateResult(
            new RegisterResponse
            {
                Username = registerRequest.Username,
                JwtToken = _jwtService.GenerateJwtToken(newUser)
            });
    }

    public async Task<ServiceResult<LoginResponse>> LoginUserAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        User? matchedUser;
        try
        {
            matchedUser = await _unitOfWork.Users.GetFirstByFilterAsync(u => u.Username == loginRequest.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when checking existing user!");

            return ServiceResultFactory<LoginResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when logging in, please try again later");
        }

        if (matchedUser == null)
        {
            return ServiceResultFactory<LoginResponse>.CreateError(HttpStatusCode.Unauthorized);
        }

        if (_passwordHasher.CompareHashedPassword(matchedUser.Password, loginRequest.Password))
        {
            return ServiceResultFactory<LoginResponse>.CreateResult(new LoginResponse
            {
                Username = matchedUser.Username,
                JwtToken = _jwtService.GenerateJwtToken(matchedUser)
            });
        }

        return ServiceResultFactory<LoginResponse>.CreateError(HttpStatusCode.Unauthorized);
    }

    public async Task<ServiceResult<GetUserResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        User? matchedUser;
        
        try
        {
            matchedUser = await _unitOfWork.Users.GetFirstByFilterAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when checking existing user!");

            return ServiceResultFactory<GetUserResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (matchedUser == null)
        {
            return ServiceResultFactory<GetUserResponse>.CreateError(HttpStatusCode.NotFound);
        }

        return ServiceResultFactory<GetUserResponse>.CreateResult(_userMapper.MapToGetUserResponse(matchedUser));
    }
}
