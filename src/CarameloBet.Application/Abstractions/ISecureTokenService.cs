namespace CarameloBet.Application.Abstractions;

public interface ISecureTokenService
{
    string GenerateToken();
    string HashToken(string token);
}
