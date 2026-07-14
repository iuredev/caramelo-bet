using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.UseCases.Auth;

internal static class UserAccess
{
    public static async Task<bool> IsActiveAsync(
        User user,
        IUserRepository userRepository)
    {
        if (user.ReleaseExpiredBlock(DateTime.UtcNow))
        {
            await userRepository.SaveChangesAsync();
        }

        return user.Status == "active";
    }
}
