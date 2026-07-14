using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class RefreshTokenUseCase(
    IUserRepository userRepository,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
{
    public async Task<RefreshTokenResponse> ExecuteAsync(RefreshTokenRequest request)
    {
        var tokenHash = jwtService.HashRefreshToken(request.RefreshToken);
        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenHash = jwtService.HashRefreshToken(refreshToken);
        var storedToken = await refreshTokenRepository.RotateAsync(tokenHash, refreshTokenHash);

        if (storedToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = await userRepository.GetByIdAsync(storedToken.UserId);

        if (user is null || !await UserAccess.IsActiveAsync(user, userRepository))
        {
            storedToken.Revoke();
            await refreshTokenRepository.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var roles = await userRepository.GetRolesAsync(user.Id);
        var permissions = await userRepository.GetPermissionsAsync(user.Id);
        var accessToken = jwtService.GenerateAccessToken(user, roles, permissions);

        return new RefreshTokenResponse(
            accessToken,
            refreshToken);
    }
}
