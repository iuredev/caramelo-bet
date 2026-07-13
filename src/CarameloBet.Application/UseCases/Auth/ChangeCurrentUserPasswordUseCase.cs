using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class ChangeCurrentUserPasswordUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
{
    public async Task ExecuteAsync(Guid userId, ChangeCurrentUserPasswordRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId);

        if (user is null || user.Status != "active")
        {
            throw new UnauthorizedAccessException("Invalid user");
        }

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid current password");
        }

        user.ChangePassword(passwordHasher.HashPassword(request.NewPassword));
        await userRepository.SaveChangesAsync();
    }
}
