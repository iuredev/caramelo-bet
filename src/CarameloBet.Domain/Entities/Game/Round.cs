namespace CarameloBet.Domain.Entities.Game;

public class Round
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public string Status { get; private set; } = "betting";
    public int? Result { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? BettingClosedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Round() { }

    public static Round Create(Guid tableId)
    {

        return new Round
        {
            Id = Guid.NewGuid(),
            TableId = tableId,
            Status = "betting",
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void CloseBetting()
    {
        Status = "spinning";
        BettingClosedAt = DateTime.UtcNow;
    }

    public void SetResult(int result)
    {
        Result = result;
        Status = "playing";
    }

    public void Finish(int result)
    {
        Status = "finished";
        Result = result;
        FinishedAt = DateTime.UtcNow;
    }
}
