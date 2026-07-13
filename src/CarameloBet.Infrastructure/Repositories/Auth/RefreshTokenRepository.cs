using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth;
using Microsoft.EntityFrameworkCore;

namespace CarameloBet.Infrastructure.Repositories.Auth;

public class RefreshTokenRepository(AuthDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await context.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);
    }

    public async Task<RefreshToken?> RotateAsync(string oldTokenHash, string newTokenHash)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var oldToken = await context.RefreshTokens
            .FromSqlInterpolated($"""
                SELECT *
                FROM auth.refresh_tokens
                WHERE token_hash = {oldTokenHash}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync();

        if (oldToken is null || !oldToken.IsActive)
        {
            return null;
        }

        oldToken.Revoke();
        var newToken = RefreshToken.Create(oldToken.UserId, newTokenHash);

        await context.RefreshTokens.AddAsync(newToken);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return newToken;
    }

    public async Task RevokeActiveTokensForUserAsync(Guid userId)
    {
        var activeTokens = await context.RefreshTokens
            .Where(token => token.UserId == userId
                && token.RevokedAt == null
                && token.ExpiresAt > DateTime.UtcNow)
            .ToArrayAsync();

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
