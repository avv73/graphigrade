namespace GraphiGrade.Services.Utils.Abstractions;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool CompareHashedPassword(string expectedPasswordHash, string password);
}
