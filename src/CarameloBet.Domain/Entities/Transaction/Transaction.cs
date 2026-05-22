namespace CarameloBet.Domain.Entities.Transaction;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Movement { get; private set; } = string.Empty;
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid IdempotencyKey { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transaction() { }

    public static Transaction Create(Guid walletId,
            decimal amount,
            string type,
            string movement,
            decimal balanceBefore,
            decimal balanceAfter,
            Guid idempotencyKey,
            Guid? referenceId = null,
            string? referenceType = null)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Amount = amount,
            Type = type,
            Movement = movement,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            IdempotencyKey = idempotencyKey,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            CreatedAt = DateTime.UtcNow
        };
    }
}
