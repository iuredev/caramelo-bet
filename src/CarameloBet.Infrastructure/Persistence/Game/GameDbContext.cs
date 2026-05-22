using GameEntity = CarameloBet.Domain.Entities.Game.Game;
using CarameloBet.Domain.Entities.Game;
using Microsoft.EntityFrameworkCore;
using CarameloBet.Infrastructure.Persistence.Game.Configurations;

namespace CarameloBet.Infrastructure.Persistence.Game;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<GameEntity> Games { get; init; }
    public DbSet<Table> Tables { get; init; }
    public DbSet<Round> Rounds { get; init; }
    public DbSet<Bet> Bets { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("game");
        modelBuilder.ApplyConfiguration(new GameConfiguration());
        modelBuilder.ApplyConfiguration(new TableConfiguration());
        modelBuilder.ApplyConfiguration(new RoundConfiguration());
        modelBuilder.ApplyConfiguration(new BetConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
