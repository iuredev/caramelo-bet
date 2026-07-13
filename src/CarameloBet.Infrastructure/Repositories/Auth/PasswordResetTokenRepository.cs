using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth;
using Microsoft.EntityFrameworkCore;

namespace CarameloBet.Infrastructure.Repositories.Auth;

public class PasswordResetTokenRepository(AuthDbContext context) : IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await context.PasswordResetTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);
    }

    public async Task AddAsync(PasswordResetToken passwordResetToken)
    {
        await context.PasswordResetTokens.AddAsync(passwordResetToken);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
