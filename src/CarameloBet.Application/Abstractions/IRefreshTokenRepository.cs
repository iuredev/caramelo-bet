using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<RefreshToken?> RotateAsync(string oldTokenHash, string newTokenHash);
    Task RevokeActiveTokensForUserAsync(Guid userId);
    Task AddAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}
