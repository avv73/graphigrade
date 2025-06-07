using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IJwtService
{
    string GenerateJwtToken(User user);
}
