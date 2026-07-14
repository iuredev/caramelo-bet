using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Domain.Tests;

public class AuthEntityTests
{
    [Fact]
    public void UserCreateInitializesActiveUser()
    {
        var birthdate = new DateOnly(1990, 5, 10);

        var user = User.Create("Caramelo Player", "player@example.com", "password-hash", birthdate);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Caramelo Player", user.Name);
        Assert.Equal("player@example.com", user.Email);
        Assert.Equal("password-hash", user.PasswordHash);
        Assert.Equal(birthdate, user.Birthdate);
        Assert.Equal("active", user.Status);
        Assert.NotEqual(default, user.CreatedAt);
        Assert.NotEqual(default, user.UpdatedAt);
    }

    [Fact]
    public void UserChangePasswordReplacesHash()
    {
        var user = User.Create("Player", "player@example.com", "old-hash");

        user.ChangePassword("new-hash");

        Assert.Equal("new-hash", user.PasswordHash);
        Assert.True(user.UpdatedAt >= user.CreatedAt);
    }

    [Fact]
    public void UserUpdateProfileReplacesEditableFields()
    {
        var user = User.Create("Old Name", "old@example.com", "hash");
        var birthdate = new DateOnly(1985, 3, 15);

        user.UpdateProfile("New Name", "new@example.com", birthdate);

        Assert.Equal("New Name", user.Name);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal(birthdate, user.Birthdate);
        Assert.True(user.UpdatedAt >= user.CreatedAt);
    }

    [Fact]
    public void UserUpdateStatusChangesAccountStatus()
    {
        var user = User.Create("Player", "player@example.com", "hash");

        user.UpdateStatus("suspended");

        Assert.Equal("suspended", user.Status);
        Assert.True(user.UpdatedAt >= user.CreatedAt);
    }

    [Fact]
    public void UserBlockStoresModerationDetails()
    {
        var user = User.Create("Player", "player@example.com", "hash");
        var blockedUntil = DateTime.UtcNow.AddDays(1);

        user.Block("Responsible gaming review", blockedUntil);

        Assert.Equal("blocked", user.Status);
        Assert.Equal("Responsible gaming review", user.BlockedReason);
        Assert.Equal(blockedUntil, user.BlockedUntil);
    }

    [Fact]
    public void UserReleaseExpiredBlockReactivatesUser()
    {
        var user = User.Create("Player", "player@example.com", "hash");
        user.Block("Temporary review", DateTime.UtcNow.AddMinutes(1));

        var released = user.ReleaseExpiredBlock(DateTime.UtcNow.AddMinutes(2));

        Assert.True(released);
        Assert.Equal("active", user.Status);
        Assert.Null(user.BlockedReason);
        Assert.Null(user.BlockedUntil);
    }

    [Fact]
    public void RoleCreateAndUpdateMaintainsIdentity()
    {
        var role = Role.Create("support", "Support agent");
        var id = role.Id;

        role.Update("moderator", "Game moderator");

        Assert.Equal(id, role.Id);
        Assert.Equal("moderator", role.Name);
        Assert.Equal("Game moderator", role.Description);
        Assert.NotEqual(default, role.CreatedAt);
    }

    [Fact]
    public void PermissionCreateInitializesPermission()
    {
        var permission = Permission.Create("can_play_games", "Can play games");

        Assert.NotEqual(Guid.Empty, permission.Id);
        Assert.Equal("can_play_games", permission.Name);
        Assert.Equal("Can play games", permission.Description);
        Assert.NotEqual(default, permission.CreatedAt);
    }

    [Fact]
    public void UserRoleCreateLinksUserAndRole()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRole = UserRole.Create(userId, roleId);

        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
        Assert.NotEqual(default, userRole.CreatedAt);
    }

    [Fact]
    public void RolePermissionCreateLinksRoleAndPermission()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermission = RolePermission.Create(roleId, permissionId);

        Assert.Equal(roleId, rolePermission.RoleId);
        Assert.Equal(permissionId, rolePermission.PermissionId);
        Assert.NotEqual(default, rolePermission.CreatedAt);
    }

    [Fact]
    public void RefreshTokenCreateProducesActiveToken()
    {
        var userId = Guid.NewGuid();

        var token = RefreshToken.Create(userId, "token-hash");

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal("token-hash", token.TokenHash);
        Assert.False(token.IsExpired);
        Assert.False(token.IsRevoked);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void RefreshTokenRevokeMakesTokenInactive()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "token-hash");

        token.Revoke();

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public void ExpiredRefreshTokenIsInactive()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "token-hash", expiryDays: -1);

        Assert.True(token.IsExpired);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void PasswordResetTokenCreateProducesValidToken()
    {
        var userId = Guid.NewGuid();

        var token = PasswordResetToken.Create(userId, "token-hash");

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal("token-hash", token.TokenHash);
        Assert.False(token.IsExpired);
        Assert.False(token.IsUsed);
        Assert.True(token.IsValid);
    }

    [Fact]
    public void PasswordResetTokenMarkedAsUsedBecomesInvalid()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token-hash");

        token.MarkAsUsed();

        Assert.True(token.IsUsed);
        Assert.False(token.IsValid);
        Assert.NotNull(token.UsedAt);
    }

    [Fact]
    public void ExpiredPasswordResetTokenIsInvalid()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token-hash", expiryHours: -1);

        Assert.True(token.IsExpired);
        Assert.False(token.IsValid);
    }
}
