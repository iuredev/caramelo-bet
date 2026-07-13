using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Abstractions;

public interface IRoleRepository
{
    Task<IReadOnlyCollection<Role>> ListAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task<bool> NameExistsAsync(string name);
    Task<bool> NameExistsForOtherRoleAsync(Guid roleId, string name);
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId);
    Task AddAsync(Role role);
    Task AssignRoleAsync(Guid userId, Guid roleId);
    Task SaveChangesAsync();
}
