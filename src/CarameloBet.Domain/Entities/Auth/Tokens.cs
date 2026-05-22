namespace CarameloBet.Domain.Entities.Auth;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, int expiryDays = 7)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}

public class PasswordResetToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsValid => !IsExpired && !IsUsed;

    private PasswordResetToken() { }

    public static PasswordResetToken Create(Guid userId, string tokenHash, int expiryHours = 1)
    {
        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsUsed() => UsedAt = DateTime.UtcNow;
}
