using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GameEntity = CarameloBet.Domain.Entities.Game.Game;
using TableEntity = CarameloBet.Domain.Entities.Game.Table;

namespace CarameloBet.Infrastructure.Persistence.Game.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<GameEntity>
{
    public void Configure(EntityTypeBuilder<GameEntity> builder)
    {
        builder.ToTable("games");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.MinBet)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(g => g.MaxBet)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
    }
}

public class TableConfiguration : IEntityTypeConfiguration<TableEntity>
{
    public void Configure(EntityTypeBuilder<TableEntity> builder)
    {
        builder.ToTable("tables");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("active");

        builder.HasIndex(t => t.GameId)
            .HasDatabaseName("idx_tables_game_id");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("idx_tables_status");
    }
}
