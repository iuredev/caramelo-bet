using System.Security.Cryptography;
using System.Text;
using CarameloBet.Application.Abstractions;

namespace CarameloBet.Infrastructure.Services.Auth;

public class SecureTokenService : ISecureTokenService
{
    private const int TokenBytes = 64;

    public string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenBytes));
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
