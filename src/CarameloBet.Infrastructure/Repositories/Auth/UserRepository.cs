using CarameloBet.Application.Abstractions;
using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Domain.Entities.Wallet;
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
    public async Task RegisterPlayerAsync(User user)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var playerRole = await context.Roles
            .SingleOrDefaultAsync(role => role.Name == "player")
            ?? throw new InvalidOperationException("The player role is not configured.");

        var wallet = Wallet.Create(user.Id);

        await context.Users.AddAsync(user);
        await context.UserRoles.AddAsync(UserRole.Create(user.Id, playerRole.Id));
        await context.SaveChangesAsync();

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO wallet.wallets
                (id, player_id, balance, currency, version, created_at, updated_at)
            VALUES
                ({wallet.Id}, {wallet.PlayerId}, {wallet.Balance}, {wallet.Currency},
                 {wallet.Version}, {wallet.CreatedAt}, {wallet.UpdatedAt})
            """);

        await transaction.CommitAsync();
    }

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
