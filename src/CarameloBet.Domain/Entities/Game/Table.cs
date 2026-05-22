namespace CarameloBet.Domain.Entities.Game;

public class Table
{
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Status { get; private set; } = "active";
    public int? MaxPlayers { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Table() { }

    public static Table Create(Guid gameId, string name, int? maxPlayers = null)
    {
        return new Table
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            Name = name,
            Status = "active",
            MaxPlayers = maxPlayers,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => Status = "inactive";
    public void Activate() => Status = "active";
    public void SetMaintenance() => Status = "maintenance";
}
