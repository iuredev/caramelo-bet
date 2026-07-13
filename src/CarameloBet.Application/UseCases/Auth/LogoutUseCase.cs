using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class LogoutUseCase(
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
{
    public async Task ExecuteAsync(LogoutRequest request)
    {
        var tokenHash = jwtService.HashRefreshToken(request.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null || storedToken.IsRevoked)
        {
            return;
        }

        storedToken.Revoke();
        await refreshTokenRepository.SaveChangesAsync();
    }
}
