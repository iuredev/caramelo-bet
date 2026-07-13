using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class UpdateCurrentUserUseCase(IUserRepository userRepository)
{
    public async Task<UserProfileResponse> ExecuteAsync(Guid userId, UpdateCurrentUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId);

        if (user is null || user.Status != "active")
        {
            throw new UnauthorizedAccessException("Invalid user");
        }

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && await userRepository.EmailExistsAsync(request.Email))
        {
            throw new ApplicationException("Email already exists");
        }

        user.UpdateProfile(request.Name, request.Email, request.BirthDate);
        await userRepository.SaveChangesAsync();

        return new UserProfileResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Birthdate,
            user.Status,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
