using CarameloBet.Domain.Entities.History;
using CarameloBet.Infrastructure.Persistence.History.Configurations;

using Microsoft.EntityFrameworkCore;

namespace CarameloBet.Infrastructure.Persistence.History;

public class HistoryDbContext(DbContextOptions<HistoryDbContext> options) : DbContext(options)
{
    public DbSet<HistoryEvent> Events { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("history");
        modelBuilder.ApplyConfiguration(new HistoryEventConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
