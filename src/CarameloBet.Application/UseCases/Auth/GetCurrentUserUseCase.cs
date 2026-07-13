using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class GetCurrentUserUseCase(IUserRepository userRepository)
{
    public async Task<UserProfileResponse> ExecuteAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);

        if (user is null || user.Status != "active")
        {
            throw new UnauthorizedAccessException("Invalid user");
        }

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
