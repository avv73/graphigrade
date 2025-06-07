using GraphiGrade.Business.Services.Utils.Abstractions;

namespace GraphiGrade.Business.Services.Utils;


public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        return passwordHash;
    }

    public bool CompareHashedPassword(string expectedPasswordHash, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, expectedPasswordHash);
    }
}
