using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth;

namespace CarameloBet.Infrastructure.Repositories.Auth;

public class RefreshTokenRepository(AuthDbContext context) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
