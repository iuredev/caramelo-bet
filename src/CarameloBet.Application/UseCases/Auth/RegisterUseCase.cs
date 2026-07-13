using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class RegisterUseCase(IUserRepository userRepo, IPasswordHasher passwordHasher)
{
    public async Task<RegisterResponse> ExecuteAsync(RegisterRequest request)
    {
        if (await userRepo.EmailExistsAsync(request.Email)) throw new ApplicationException("Email already exists");

        var passwordHash = passwordHasher.HashPassword(request.Password);

        var user = User.Create(request.Name, request.Email, passwordHash, request.BirthDate);

        await userRepo.RegisterPlayerAsync(user);

        return new RegisterResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Birthdate,
            user.Status,
            user.CreatedAt
        );
    }
}
