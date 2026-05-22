using CarameloBet.Domain.Entities.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarameloBet.Infrastructure.Persistence.History.Configurations;

public class HistoryEventConfiguration : IEntityTypeConfiguration<HistoryEvent>
{
    public void Configure(EntityTypeBuilder<HistoryEvent> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Payload)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("idx_events_event_type");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_events_user_id");

        builder.HasIndex(e => e.RoundId)
            .HasDatabaseName("idx_events_round_id");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("idx_events_created_at");
    }
}
