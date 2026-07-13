using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Abstractions;

public interface IAdminUserRepository
{
    Task<IReadOnlyCollection<User>> ListAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId);
    Task<bool> EmailExistsForOtherUserAsync(Guid userId, string email);
    Task SaveChangesAsync();
}
