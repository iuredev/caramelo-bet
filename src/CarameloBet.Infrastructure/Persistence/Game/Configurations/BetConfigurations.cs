using CarameloBet.Domain.Entities.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarameloBet.Infrastructure.Persistence.Game.Configurations;

public class BetConfiguration : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> builder)
    {
        builder.ToTable("bets");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.BetType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.BetValue)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("pending");

        builder.Property(b => b.Payout)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(b => b.RoundId)
            .HasDatabaseName("idx_bets_round_id");

        builder.HasIndex(b => b.PlayerId)
            .HasDatabaseName("idx_bets_player_id");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("idx_bets_status");
    }
}
