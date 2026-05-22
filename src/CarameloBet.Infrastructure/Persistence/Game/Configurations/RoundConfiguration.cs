using CarameloBet.Domain.Entities.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarameloBet.Infrastructure.Persistence.Game.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("rounds");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("betting");

        builder.HasIndex(r => r.TableId)
            .HasDatabaseName("idx_rounds_table_id");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("idx_rounds_status");

        builder.HasIndex(r => r.StartedAt)
            .HasDatabaseName("idx_rounds_started_at");
    }
}
