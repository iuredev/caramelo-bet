using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CarameloBet.Infrastructure.Persistence.Auth;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Role> Roles { get; init; }
    public DbSet<Permission> Permissions { get; init; }
    public DbSet<UserRole> UserRoles { get; init; }
    public DbSet<RolePermission> RolePermissions { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
        base.OnModelCreating(modelBuilder);
    }
};
