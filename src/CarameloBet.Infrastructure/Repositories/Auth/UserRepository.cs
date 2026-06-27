using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth;
using Microsoft.EntityFrameworkCore;

namespace CarameloBet.Infrastructure.Repositories.Auth;

public class UserRepository(AuthDbContext context) : IUserRepository
{

    public async Task<User?> GetByIdAsync(Guid id) => await context.Users.FindAsync(id);
    public async Task<User?> GetByEmailAsync(string email) => await context.Users.FirstOrDefaultAsync(usr => usr.Email == email);
    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId)
    {
        return await (
            from userRole in context.UserRoles
            join role in context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId)
    {
        return await (
            from userRole in context.UserRoles
            join rolePermission in context.RolePermissions on userRole.RoleId equals rolePermission.RoleId
            join permission in context.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
            select permission.Name)
            .Distinct()
            .ToArrayAsync();
    }

    public async Task<bool> EmailExistsAsync(string email) => await context.Users.AnyAsync(usr => usr.Email == email);
    public async Task AddAsync(User user) => await context.Users.AddAsync(user);
    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
