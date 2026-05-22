namespace CarameloBet.Domain.Entities.Game;

public class Game
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public decimal MinBet { get; private set; }
    public decimal MaxBet { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Game() { }

    public static Game Create(string name, string type, decimal minBet, decimal maxBet)
    {
        return new Game
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            MinBet = minBet,
            MaxBet = maxBet,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
