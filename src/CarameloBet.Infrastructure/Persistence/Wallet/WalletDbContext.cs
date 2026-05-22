using WalletEntity = CarameloBet.Domain.Entities.Wallet.Wallet;
using CarameloBet.Domain.Entities.Transaction;
using Microsoft.EntityFrameworkCore;
using CarameloBet.Infrastructure.Persistence.Wallet.Configurations;

namespace CarameloBet.Infrastructure.Persistence.Wallet;

public class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
{
    public DbSet<WalletEntity> Wallets { get; init; }
    public DbSet<Transaction> Transactions { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("wallet");
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        base.OnModelCreating(modelBuilder);
    }
};
