using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Abstractions;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(PasswordResetToken passwordResetToken);
    Task SaveChangesAsync();
}
