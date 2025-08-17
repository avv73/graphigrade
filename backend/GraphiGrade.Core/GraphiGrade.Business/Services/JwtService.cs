using GraphiGrade.Business.Authorization;
using GraphiGrade.Business.Configurations;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Data.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GraphiGrade.Business.Services;

public class JwtService : IJwtService
{
    private readonly GraphiGradeConfig _config;

    private static readonly JwtSecurityTokenHandler JwtHandler = new();

    public JwtService(IOptions<GraphiGradeConfig> config)
    {
        _config = config.Value;
    }

    public string GenerateJwtToken(User user)
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.SerialNumber, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.IsTeacher ? Role.AdminRole : Role.StudentRole)
        ];

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_config.JwtSecretKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha512);

        JwtSecurityToken token = new(
            issuer: _config.JwtIssuer,
            audience: _config.JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddSeconds(-1),
            expires: DateTime.UtcNow.AddSeconds(_config.JwtExpirationInSeconds),
            signingCredentials: credentials);

        return JwtHandler.WriteToken(token);
    }
}
