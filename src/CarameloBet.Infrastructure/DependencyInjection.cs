using CarameloBet.Application.Abstractions;
using CarameloBet.Infrastructure.Persistence.Auth;
using CarameloBet.Infrastructure.Persistence.Game;
using CarameloBet.Infrastructure.Persistence.History;
using CarameloBet.Infrastructure.Persistence.Wallet;
using CarameloBet.Infrastructure.Repositories.Auth;
using CarameloBet.Infrastructure.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace CarameloBet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseContexts(configuration);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ISecureTokenService, SecureTokenService>();
        services.AddScoped<IPasswordResetEmailSender, PasswordResetEmailSender>();
        services.AddTransient<IResend>(_ =>
            ResendClient.Create(configuration["Resend:ApiToken"] ?? string.Empty));

        return services;
    }

    private static IServiceCollection AddDatabaseContexts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSQL is required.");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString,
                    o => o.MigrationsHistoryTable("__auth_migrations_history"))
                .UseSnakeCaseNamingConvention());

        services.AddDbContext<WalletDbContext>(options =>
            options.UseNpgsql(connectionString,
                    o => o.MigrationsHistoryTable("__wallet_migrations_history"))
                .UseSnakeCaseNamingConvention());

        services.AddDbContext<GameDbContext>(options =>
            options.UseNpgsql(connectionString,
                    o => o.MigrationsHistoryTable("__game_migrations_history"))
                .UseSnakeCaseNamingConvention());

        services.AddDbContext<HistoryDbContext>(options =>
            options.UseNpgsql(connectionString,
                    o => o.MigrationsHistoryTable("__history_migrations_history"))
                .UseSnakeCaseNamingConvention());

        return services;
    }
}
