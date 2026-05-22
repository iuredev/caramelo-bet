using CarameloBet.Domain.Exceptions;

namespace CarameloBet.Domain.Entities.Wallet;

public class Wallet
{
    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = "credits";
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Wallet() { }

    public static Wallet Create(Guid playerId, decimal initialBalance = 0m)
    {
        return new Wallet
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Balance = initialBalance,
            Currency = "credits",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        if (Balance < amount) throw new InsufficientBalanceException(Balance, amount);

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }
}
