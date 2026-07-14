using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class ResetPasswordUseCase(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ISecureTokenService secureTokenService,
    IPasswordHasher passwordHasher)
{
    public async Task ExecuteAsync(ResetPasswordRequest request)
    {
        var tokenHash = secureTokenService.HashToken(request.Token);
        var storedToken = await passwordResetTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null || !storedToken.IsValid)
        {
            throw new UnauthorizedAccessException("Invalid password reset token");
        }

        var user = await userRepository.GetByIdAsync(storedToken.UserId);

        if (user is null || !await UserAccess.IsActiveAsync(user, userRepository))
        {
            throw new UnauthorizedAccessException("Invalid password reset token");
        }

        user.ChangePassword(passwordHasher.HashPassword(request.NewPassword));
        storedToken.MarkAsUsed();
        await refreshTokenRepository.RevokeActiveTokensForUserAsync(user.Id);

        await userRepository.SaveChangesAsync();
        await passwordResetTokenRepository.SaveChangesAsync();
        await refreshTokenRepository.SaveChangesAsync();
    }
}
