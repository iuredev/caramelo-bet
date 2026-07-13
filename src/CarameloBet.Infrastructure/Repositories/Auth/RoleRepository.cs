using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth;
using Microsoft.EntityFrameworkCore;

namespace CarameloBet.Infrastructure.Repositories.Auth;

public class RoleRepository(AuthDbContext context) : IRoleRepository
{
    public async Task<IReadOnlyCollection<Role>> ListAsync()
    {
        return await context.Roles
            .OrderBy(role => role.Name)
            .ToArrayAsync();
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await context.Roles.FindAsync(id);
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await context.Roles.AnyAsync(role => role.Name == name);
    }

    public async Task<bool> NameExistsForOtherRoleAsync(Guid roleId, string name)
    {
        return await context.Roles
            .AnyAsync(role => role.Id != roleId && role.Name == name);
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId)
    {
        return await context.UserRoles
            .AnyAsync(userRole => userRole.UserId == userId && userRole.RoleId == roleId);
    }

    public async Task AddAsync(Role role)
    {
        await context.Roles.AddAsync(role);
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId)
    {
        await context.UserRoles.AddAsync(UserRole.Create(userId, roleId));
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
