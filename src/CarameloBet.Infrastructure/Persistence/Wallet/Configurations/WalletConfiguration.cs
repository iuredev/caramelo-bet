using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletEntity = CarameloBet.Domain.Entities.Wallet.Wallet;

namespace CarameloBet.Infrastructure.Persistence.Wallet.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<WalletEntity>
{
    public void Configure(EntityTypeBuilder<WalletEntity> builder)
    {
        builder.ToTable("wallets");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Balance)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("credits");

        builder.Property(w => w.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasIndex(w => w.PlayerId)
            .IsUnique()
            .HasDatabaseName("idx_wallets_player_id");
    }
}
