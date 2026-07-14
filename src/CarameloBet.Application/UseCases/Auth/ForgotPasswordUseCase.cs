using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class ForgotPasswordUseCase(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    ISecureTokenService secureTokenService,
    IPasswordResetEmailSender passwordResetEmailSender)
{
    public async Task ExecuteAsync(ForgotPasswordRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);

        if (user is null || !await UserAccess.IsActiveAsync(user, userRepository))
        {
            return;
        }

        var resetToken = secureTokenService.GenerateToken();
        var resetTokenHash = secureTokenService.HashToken(resetToken);

        await passwordResetTokenRepository.AddAsync(
            PasswordResetToken.Create(user.Id, resetTokenHash));
        await passwordResetTokenRepository.SaveChangesAsync();
        await passwordResetEmailSender.SendAsync(user.Email, resetToken);
    }
}
