namespace CarameloBet.Domain.Entities.Auth;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateOnly? Birthdate { get; private set; }
    public string Status { get; private set; } = "active";
    public string? BlockedReason { get; private set; }
    public DateTime? BlockedUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User() { }

    public static User Create(string name, string email, string passwordHash, DateOnly? birthdate = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            Birthdate = birthdate,
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string name, string email, DateOnly? birthdate)
    {
        Name = name;
        Email = email;
        Birthdate = birthdate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(string status)
    {
        if (status == "blocked")
        {
            throw new InvalidOperationException("Use Block to block a user.");
        }

        Status = status;
        BlockedReason = null;
        BlockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block(string reason, DateTime? blockedUntil)
    {
        if (Status == "deleted")
        {
            throw new InvalidOperationException("A deleted user cannot be blocked.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Block reason is required.", nameof(reason));
        }

        if (blockedUntil.HasValue && blockedUntil.Value <= DateTime.UtcNow)
        {
            throw new ArgumentException("Block expiration must be in the future.", nameof(blockedUntil));
        }

        Status = "blocked";
        BlockedReason = reason.Trim();
        BlockedUntil = blockedUntil?.ToUniversalTime();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unblock()
    {
        if (Status == "deleted")
        {
            throw new InvalidOperationException("A deleted user cannot be unblocked.");
        }

        Status = "active";
        BlockedReason = null;
        BlockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ReleaseExpiredBlock(DateTime utcNow)
    {
        if (Status != "blocked" || !BlockedUntil.HasValue || BlockedUntil.Value > utcNow)
        {
            return false;
        }

        Unblock();
        return true;
    }
};
