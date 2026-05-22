using System.Text.Json;

namespace CarameloBet.Domain.Entities.History;

public class HistoryEvent
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public Guid? TableId { get; private set; }
    public Guid? RoundId { get; private set; }
    public Guid? BetId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private HistoryEvent() { }

    public static HistoryEvent Create(
        string eventType,
        object payload,
        Guid? userId = null,
        Guid? tableId = null,
        Guid? roundId = null,
        Guid? betId = null)
    {
        return new HistoryEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            UserId = userId,
            TableId = tableId,
            RoundId = roundId,
            BetId = betId,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };
    }

}
