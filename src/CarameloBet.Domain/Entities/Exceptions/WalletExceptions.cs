namespace CarameloBet.Domain.Exceptions;

public class InsufficientBalanceException : Exception
{
    public decimal CurrentBalance { get; }
    public decimal RequiredAmount { get; }

    public InsufficientBalanceException(decimal currentBalance, decimal requiredAmount)
        : base($"Insufficient balance. Current: {currentBalance}, Required: {requiredAmount}")
    {
        CurrentBalance = currentBalance;
        RequiredAmount = requiredAmount;
    }
}
