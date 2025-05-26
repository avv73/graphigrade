using GraphiGrade.DTOs.User.Responses;
using GraphiGrade.Models;

namespace GraphiGrade.Mappers.Abstractions;

public interface IUserMapper
{
    GetUserResponse MapToGetUserResponse(User user);
}
