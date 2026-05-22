using CarameloBet.Infrastructure.Persistence.Auth;
using CarameloBet.Infrastructure.Persistence.Wallet;
using CarameloBet.Infrastructure.Persistence.Game;
using CarameloBet.Infrastructure.Persistence.History;
using Microsoft.EntityFrameworkCore;

namespace CarameloBet.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseContexts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<WalletDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<GameDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<HistoryDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
