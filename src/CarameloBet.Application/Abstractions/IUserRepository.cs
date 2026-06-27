using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Abstractions;


public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId);
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
