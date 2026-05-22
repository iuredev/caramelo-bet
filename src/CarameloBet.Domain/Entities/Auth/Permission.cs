namespace CarameloBet.Domain.Entities.Auth;

public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Permission() { }

    public static Permission Create(string name, string description)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}
