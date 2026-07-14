using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Application.UseCases.Auth;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Tests;

public class AdminUseCaseTests
{
    [Fact]
    public async Task UpdateAdminUser_WithTakenEmail_ThrowsApplicationException()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var otherUser = User.Create("Other", "other@example.com", "hashed-password");
        var useCase = new UpdateAdminUserUseCase(new FakeAdminUserRepository(user, otherUser));

        await Assert.ThrowsAsync<ApplicationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                new UpdateAdminUserRequest("Iure", otherUser.Email, null)));
    }

    [Fact]
    public async Task UpdateAdminUser_WithValidData_UpdatesProfile()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var useCase = new UpdateAdminUserUseCase(new FakeAdminUserRepository(user));

        var response = await useCase.ExecuteAsync(
            user.Id,
            new UpdateAdminUserRequest("Iure Silva", "iure.silva@example.com", null));

        Assert.Equal("Iure Silva", user.Name);
        Assert.Equal("iure.silva@example.com", user.Email);
        Assert.Equal("active", user.Status);
        Assert.Equal(user.Status, response.Status);
    }

    [Fact]
    public async Task BlockAdminUser_WithReason_BlocksUserAndRevokesRefreshTokens()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var refreshToken = RefreshToken.Create(user.Id, "refresh-token-hash");
        var refreshTokens = new FakeRefreshTokenRepository(refreshToken);
        var useCase = new BlockAdminUserUseCase(
            new FakeAdminUserRepository(user),
            refreshTokens);
        var expiresAt = DateTime.UtcNow.AddHours(2);

        var response = await useCase.ExecuteAsync(
            user.Id,
            new BlockUserRequest("Chargeback investigation", expiresAt));

        Assert.Equal("blocked", user.Status);
        Assert.Equal("Chargeback investigation", user.BlockedReason);
        Assert.Equal(expiresAt, user.BlockedUntil);
        Assert.Equal(user.BlockedReason, response.BlockedReason);
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public async Task UnblockAdminUser_WithBlockedUser_ClearsBlockState()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        user.Block("Manual review", null);
        var useCase = new UnblockAdminUserUseCase(new FakeAdminUserRepository(user));

        var response = await useCase.ExecuteAsync(user.Id);

        Assert.Equal("active", user.Status);
        Assert.Null(user.BlockedReason);
        Assert.Null(user.BlockedUntil);
        Assert.Equal("active", response.Status);
    }

    [Fact]
    public async Task DeleteAdminUser_WithExistingUser_MarksUserDeleted()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var useCase = new DeleteAdminUserUseCase(new FakeAdminUserRepository(user));

        await useCase.ExecuteAsync(user.Id);

        Assert.Equal("deleted", user.Status);
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ThrowsApplicationException()
    {
        var role = Role.Create("admin", "Administrator");
        var useCase = new CreateRoleUseCase(new FakeRoleRepository(role));

        await Assert.ThrowsAsync<ApplicationException>(() =>
            useCase.ExecuteAsync(new CreateRoleRequest(role.Name, "Duplicate")));
    }

    [Fact]
    public async Task AssignUserRole_WithMissingRole_ThrowsKeyNotFound()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var useCase = new AssignUserRoleUseCase(
            new FakeAdminUserRepository(user),
            new FakeRoleRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(user.Id, new AssignRoleRequest(Guid.NewGuid())));
    }

    [Fact]
    public async Task AssignUserRole_WithExistingRole_AddsUserRole()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var role = Role.Create("support", "Support");
        var roles = new FakeRoleRepository(role);
        var useCase = new AssignUserRoleUseCase(new FakeAdminUserRepository(user), roles);

        await useCase.ExecuteAsync(user.Id, new AssignRoleRequest(role.Id));

        Assert.Contains((user.Id, role.Id), roles.AssignedRoles);
    }

    private sealed class FakeAdminUserRepository(params User[] users) : IAdminUserRepository
    {
        public Task<IReadOnlyCollection<User>> ListAsync()
        {
            return Task.FromResult<IReadOnlyCollection<User>>(users);
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(users.FirstOrDefault(user => user.Id == id));
        }

        public Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId)
        {
            return Task.FromResult<IReadOnlyCollection<string>>(["player"]);
        }

        public Task<bool> EmailExistsForOtherUserAsync(Guid userId, string email)
        {
            return Task.FromResult(users.Any(user => user.Id != userId && user.Email == email));
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRoleRepository(params Role[] roles) : IRoleRepository
    {
        public List<(Guid UserId, Guid RoleId)> AssignedRoles { get; } = [];

        public Task<IReadOnlyCollection<Role>> ListAsync()
        {
            return Task.FromResult<IReadOnlyCollection<Role>>(roles);
        }

        public Task<Role?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(roles.FirstOrDefault(role => role.Id == id));
        }

        public Task<bool> NameExistsAsync(string name)
        {
            return Task.FromResult(roles.Any(role => role.Name == name));
        }

        public Task<bool> NameExistsForOtherRoleAsync(Guid roleId, string name)
        {
            return Task.FromResult(roles.Any(role => role.Id != roleId && role.Name == name));
        }

        public Task<bool> UserHasRoleAsync(Guid userId, Guid roleId)
        {
            return Task.FromResult(AssignedRoles.Contains((userId, roleId)));
        }

        public Task AddAsync(Role role)
        {
            return Task.CompletedTask;
        }

        public Task AssignRoleAsync(Guid userId, Guid roleId)
        {
            AssignedRoles.Add((userId, roleId));
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRefreshTokenRepository(params RefreshToken[] tokens)
        : IRefreshTokenRepository
    {
        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return Task.FromResult(tokens.FirstOrDefault(token => token.TokenHash == tokenHash));
        }

        public Task<RefreshToken?> RotateAsync(string oldTokenHash, string newTokenHash)
        {
            return Task.FromResult<RefreshToken?>(null);
        }

        public Task RevokeActiveTokensForUserAsync(Guid userId)
        {
            foreach (var token in tokens.Where(token => token.UserId == userId && token.IsActive))
            {
                token.Revoke();
            }

            return Task.CompletedTask;
        }

        public Task AddAsync(RefreshToken refreshToken)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
