using GraphiGrade.DTOs.Auth.Requests;
using GraphiGrade.DTOs.Auth.Responses;
using GraphiGrade.DTOs.User.Responses;
using GraphiGrade.Models.ServiceModels;

namespace GraphiGrade.Services.Abstractions;

public interface IUserService
{
    Task<ServiceResult<RegisterResponse>> RegisterUserAsync(RegisterRequest registerRequest, CancellationToken cancellationToken);

    Task<ServiceResult<LoginResponse>> LoginUserAsync(LoginRequest loginRequest, CancellationToken cancellationToken);
    Task<ServiceResult<GetUserResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
}
