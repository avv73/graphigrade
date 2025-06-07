using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Contracts.DTOs.Auth.Requests;
using GraphiGrade.Contracts.DTOs.Auth.Responses;
using GraphiGrade.Contracts.DTOs.User.Responses;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IUserService
{
    Task<ServiceResult<RegisterResponse>> RegisterUserAsync(RegisterRequest registerRequest, CancellationToken cancellationToken);

    Task<ServiceResult<LoginResponse>> LoginUserAsync(LoginRequest loginRequest, CancellationToken cancellationToken);
    Task<ServiceResult<GetUserResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
}
