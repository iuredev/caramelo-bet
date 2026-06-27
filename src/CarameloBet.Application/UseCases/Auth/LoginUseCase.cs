using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class LoginUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
{
    public async Task<LoginResponse> ExecuteAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);

        Console.WriteLine("LOG2  {0}", user);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (user.Status != "active")
        {
            throw new InvalidOperationException("User is not active");
        }

        var roles = await userRepository.GetRolesAsync(user.Id);
        var permissions = await userRepository.GetPermissionsAsync(user.Id);
        var accessToken = jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenHash = jwtService.HashRefreshToken(refreshToken);

        await refreshTokenRepository.AddAsync(RefreshToken.Create(user.Id, refreshTokenHash));
        await refreshTokenRepository.SaveChangesAsync();

        return new LoginResponse(
            accessToken,
            refreshToken,
            jwtService.AccessTokenExpiresAt,
            new AuthenticatedUserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.Status));
    }
}
