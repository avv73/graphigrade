using GraphiGrade.Models;

namespace GraphiGrade.Services.Utils.Abstractions;

public interface IJwtService
{
    string GenerateJwtToken(User user);
}
