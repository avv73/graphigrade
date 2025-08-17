using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Business.Services.Utils.Abstractions;
using GraphiGrade.Contracts.DTOs.Auth.Requests;
using GraphiGrade.Contracts.DTOs.Auth.Responses;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Business.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUserMapper _userMapper;
    private readonly IExerciseMapper _exerciseMapper;
    private readonly IGroupMapper _groupMapper;

    private readonly ILogger<UserService> _logger;

    public UserService(
        IUnitOfWork unitOfWork, 
        IPasswordHasher passwordHasher, 
        IJwtService jwtService, 
        IUserMapper userMapper, 
        IExerciseMapper exerciseMapper, 
        IGroupMapper groupMapper, 
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _userMapper = userMapper;
        _exerciseMapper = exerciseMapper;
        _groupMapper = groupMapper;
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
            return ServiceResultFactory<LoginResponse>.CreateError(HttpStatusCode.Unauthorized, "Wrong username or password");
        }

        if (_passwordHasher.CompareHashedPassword(matchedUser.Password, loginRequest.Password))
        {
            return ServiceResultFactory<LoginResponse>.CreateResult(new LoginResponse
            {
                Username = matchedUser.Username,
                JwtToken = _jwtService.GenerateJwtToken(matchedUser)
            });
        }

        return ServiceResultFactory<LoginResponse>.CreateError(HttpStatusCode.Unauthorized, "Wrong username or password");
    }

    public async Task<ServiceResult<GetUserResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        User? matchedUser;

        try
        {
            matchedUser = await _unitOfWork.Users.GetFirstByFilterWithIncludesAsync(u => u.Username == username,
                query =>
                    query
                        .Include(g => g.UsersGroups)
                            .ThenInclude(ug => ug.Group)
                            .ThenInclude(g => g.ExercisesGroups)
                            .ThenInclude(eg => eg.Exercise));
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

        List<CommonResourceDto> mappedUserExercisesForUser = new();
        List<CommonResourceDto> mappedUserGroupsForUser = new();

        if (matchedUser.UsersGroups != null)
        {
            foreach (UsersGroups userUsersGroup in matchedUser.UsersGroups)
            {
                mappedUserGroupsForUser.Add(_groupMapper.MapToCommonResourceDto(userUsersGroup.Group));

                if (userUsersGroup.Group.ExercisesGroups != null)
                {
                    mappedUserExercisesForUser.AddRange(
                        userUsersGroup.Group.ExercisesGroups.Select(exercisesGroup => _exerciseMapper.MapToCommonResourceDto(exercisesGroup.Exercise)));
                }
            }
        }

        return ServiceResultFactory<GetUserResponse>.CreateResult(_userMapper.MapToGetUserResponse(matchedUser, mappedUserExercisesForUser, mappedUserGroupsForUser));
    }

    public async Task<ServiceResult<GetAllStudentsResponse>> GetAllStudentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllWithIncludesAsync(query =>
                query.Include(u => u.UsersGroups)
                     .ThenInclude(ug => ug.Group));

            var studentDtos = users
                .Where(u => !u.IsTeacher)
                .Select(u => new StudentWithGroupsDto
                {
                    Username = u.Username,
                    Id = u.Id,
                    Groups = (u.UsersGroups ?? new List<UsersGroups>())
                        .Select(ug => _groupMapper.MapToCommonResourceDto(ug.Group))
                        .ToList()
                })
                .ToList();

            return ServiceResultFactory<GetAllStudentsResponse>.CreateResult(new GetAllStudentsResponse
            {
                Users = studentDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when retrieving all student users");
            return ServiceResultFactory<GetAllStudentsResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }
    }
}
