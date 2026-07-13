using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth;
using Microsoft.EntityFrameworkCore;

namespace CarameloBet.Infrastructure.Repositories.Auth;

public class AdminUserRepository(AuthDbContext context) : IAdminUserRepository
{
    public async Task<IReadOnlyCollection<User>> ListAsync()
    {
        return await context.Users
            .OrderByDescending(user => user.CreatedAt)
            .ToArrayAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId)
    {
        return await (
            from userRole in context.UserRoles
            join role in context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name)
            .ToArrayAsync();
    }

    public async Task<bool> EmailExistsForOtherUserAsync(Guid userId, string email)
    {
        return await context.Users
            .AnyAsync(user => user.Id != userId && user.Email == email);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
