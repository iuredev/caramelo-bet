namespace CarameloBet.Domain.Entities.Game;

public class Bet
{
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public Guid PlayerId { get; private set; }
    public decimal Amount { get; private set; }
    public string BetType { get; private set; } = string.Empty;
    public string BetValue { get; private set; } = string.Empty;
    public string Status { get; private set; } = "pending";
    public decimal? Payout { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Bet() { }

    public static Bet Create(Guid roundId, Guid playerId, decimal amount, string betType, string betValue)
    {
        return new Bet
        {
            Id = Guid.NewGuid(),
            RoundId = roundId,
            PlayerId = playerId,
            Amount = amount,
            BetType = betType,
            BetValue = betValue,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsWon(decimal payout)
    {

        Payout = payout;
        Status = "won";
    }

    public void MarkAsLost()
    {
        Status = "lost";
        Payout = 0;
    }
}
