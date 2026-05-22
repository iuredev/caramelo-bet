using CarameloBet.Domain.Entities.Transaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarameloBet.Infrastructure.Persistence.Wallet.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.BalanceBefore)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.BalanceAfter)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Type)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Movement)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(t => t.WalletId)
            .HasDatabaseName("idx_transactions_wallet_id");

        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("idx_transactions_idempotency_key");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("idx_transactions_created_at");
    }
}
