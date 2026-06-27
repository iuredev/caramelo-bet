using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}
