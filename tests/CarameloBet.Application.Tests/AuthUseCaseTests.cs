using System.Reflection;
using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Application.UseCases.Auth;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.Tests;

public class AuthUseCaseTests
{
    [Fact]
    public async Task Register_WithAvailableEmail_RegistersPlayer()
    {
        var users = new FakeUserRepository();
        var useCase = new RegisterUseCase(users, new FakePasswordHasher(true));

        var response = await useCase.ExecuteAsync(
            new RegisterRequest("Iure", "iure@example.com", "StrongPass!2026", null));

        var registeredUser = Assert.Single(users.RegisteredUsers);
        Assert.Equal(registeredUser.Id, response.Id);
        Assert.Equal("Iure", registeredUser.Name);
        Assert.Equal("iure@example.com", registeredUser.Email);
        Assert.Equal("hashed-StrongPass!2026", registeredUser.PasswordHash);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsApplicationException()
    {
        var existingUser = User.Create("Iure", "iure@example.com", "hash");
        var users = new FakeUserRepository(existingUser);
        var useCase = new RegisterUseCase(users, new FakePasswordHasher(true));

        await Assert.ThrowsAsync<ApplicationException>(() =>
            useCase.ExecuteAsync(
                new RegisterRequest("Other", existingUser.Email, "StrongPass!2026", null)));

        Assert.Empty(users.RegisteredUsers);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var users = new FakeUserRepository(user);
        var refreshTokens = new FakeRefreshTokenRepository();
        var useCase = new LoginUseCase(
            users,
            new FakePasswordHasher(true),
            new FakeJwtService(),
            refreshTokens);

        var response = await useCase.ExecuteAsync(new LoginRequest(user.Email, "password"));

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token-1", response.RefreshToken);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal(user.Email, response.User.Email);
        Assert.Contains("player", response.User.Roles);
        Assert.Single(refreshTokens.Tokens);
        Assert.False(refreshTokens.Tokens[0].IsRevoked);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ThrowsUnauthorized()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var useCase = new LoginUseCase(
            new FakeUserRepository(user),
            new FakePasswordHasher(false),
            new FakeJwtService(),
            new FakeRefreshTokenRepository());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            useCase.ExecuteAsync(new LoginRequest(user.Email, "wrong-password")));
    }

    [Fact]
    public async Task Login_WithInactiveUser_ThrowsInvalidOperation()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        SetProperty(user, nameof(User.Status), "blocked");
        var useCase = new LoginUseCase(
            new FakeUserRepository(user),
            new FakePasswordHasher(true),
            new FakeJwtService(),
            new FakeRefreshTokenRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new LoginRequest(user.Email, "password")));
    }

    [Fact]
    public async Task Refresh_WithActiveToken_RotatesRefreshToken()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var oldToken = RefreshToken.Create(user.Id, "hashed:old-refresh-token");
        var refreshTokens = new FakeRefreshTokenRepository(oldToken);
        var useCase = new RefreshTokenUseCase(
            new FakeUserRepository(user),
            new FakeJwtService(),
            refreshTokens);

        var response = await useCase.ExecuteAsync(new RefreshTokenRequest("old-refresh-token"));

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token-1", response.RefreshToken);
        Assert.True(oldToken.IsRevoked);
        Assert.Equal(2, refreshTokens.Tokens.Count);
        Assert.False(refreshTokens.Tokens[1].IsRevoked);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ThrowsUnauthorized()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var revokedToken = RefreshToken.Create(user.Id, "hashed:revoked-token");
        revokedToken.Revoke();
        var useCase = new RefreshTokenUseCase(
            new FakeUserRepository(user),
            new FakeJwtService(),
            new FakeRefreshTokenRepository(revokedToken));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            useCase.ExecuteAsync(new RefreshTokenRequest("revoked-token")));
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ThrowsUnauthorized()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var expiredToken = RefreshToken.Create(user.Id, "hashed:expired-token");
        SetProperty(expiredToken, nameof(RefreshToken.ExpiresAt), DateTime.UtcNow.AddMinutes(-1));
        var useCase = new RefreshTokenUseCase(
            new FakeUserRepository(user),
            new FakeJwtService(),
            new FakeRefreshTokenRepository(expiredToken));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            useCase.ExecuteAsync(new RefreshTokenRequest("expired-token")));
    }

    [Fact]
    public async Task Logout_WithActiveToken_RevokesToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hashed:refresh-token");
        var useCase = new LogoutUseCase(
            new FakeJwtService(),
            new FakeRefreshTokenRepository(token));

        await useCase.ExecuteAsync(new LogoutRequest("refresh-token"));

        Assert.True(token.IsRevoked);
    }

    [Fact]
    public async Task Logout_WithMissingToken_DoesNotThrow()
    {
        var useCase = new LogoutUseCase(
            new FakeJwtService(),
            new FakeRefreshTokenRepository());

        await useCase.ExecuteAsync(new LogoutRequest("missing-token"));
    }

    [Fact]
    public async Task ForgotPassword_WithActiveUser_StoresTokenAndSendsEmail()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var passwordResetTokens = new FakePasswordResetTokenRepository();
        var emailSender = new FakePasswordResetEmailSender();
        var useCase = new ForgotPasswordUseCase(
            new FakeUserRepository(user),
            passwordResetTokens,
            new FakeSecureTokenService(),
            emailSender);

        await useCase.ExecuteAsync(new ForgotPasswordRequest(user.Email));

        Assert.Single(passwordResetTokens.Tokens);
        Assert.Equal("hashed:password-reset-token", passwordResetTokens.Tokens[0].TokenHash);
        Assert.Single(emailSender.Messages);
        Assert.Equal(user.Email, emailSender.Messages[0].Email);
        Assert.Equal("password-reset-token", emailSender.Messages[0].ResetToken);
    }

    [Fact]
    public async Task ForgotPassword_WithMissingUser_DoesNotStoreTokenOrSendEmail()
    {
        var passwordResetTokens = new FakePasswordResetTokenRepository();
        var emailSender = new FakePasswordResetEmailSender();
        var useCase = new ForgotPasswordUseCase(
            new FakeUserRepository(),
            passwordResetTokens,
            new FakeSecureTokenService(),
            emailSender);

        await useCase.ExecuteAsync(new ForgotPasswordRequest("missing@example.com"));

        Assert.Empty(passwordResetTokens.Tokens);
        Assert.Empty(emailSender.Messages);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ChangesPasswordAndMarksTokenUsed()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var resetToken = PasswordResetToken.Create(user.Id, "hashed:password-reset-token");
        var refreshToken = RefreshToken.Create(user.Id, "hashed:refresh-token");
        var refreshTokens = new FakeRefreshTokenRepository(refreshToken);
        var useCase = new ResetPasswordUseCase(
            new FakeUserRepository(user),
            new FakePasswordResetTokenRepository(resetToken),
            refreshTokens,
            new FakeSecureTokenService(),
            new FakePasswordHasher(true));

        await useCase.ExecuteAsync(new ResetPasswordRequest("password-reset-token", "new-password"));

        Assert.Equal("hashed-new-password", user.PasswordHash);
        Assert.True(resetToken.IsUsed);
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public async Task ResetPassword_WithUsedToken_ThrowsUnauthorized()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var resetToken = PasswordResetToken.Create(user.Id, "hashed:used-token");
        resetToken.MarkAsUsed();
        var useCase = new ResetPasswordUseCase(
            new FakeUserRepository(user),
            new FakePasswordResetTokenRepository(resetToken),
            new FakeRefreshTokenRepository(),
            new FakeSecureTokenService(),
            new FakePasswordHasher(true));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            useCase.ExecuteAsync(new ResetPasswordRequest("used-token", "new-password")));
    }

    [Fact]
    public async Task GetCurrentUser_WithActiveUser_ReturnsProfile()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var useCase = new GetCurrentUserUseCase(new FakeUserRepository(user));

        var response = await useCase.ExecuteAsync(user.Id);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal(user.Name, response.Name);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.Birthdate, response.BirthDate);
    }

    [Fact]
    public async Task UpdateCurrentUser_WithValidData_UpdatesProfile()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var birthDate = new DateOnly(1994, 1, 10);
        var useCase = new UpdateCurrentUserUseCase(new FakeUserRepository(user));

        var response = await useCase.ExecuteAsync(
            user.Id,
            new UpdateCurrentUserRequest("Iure Silva", "iure.silva@example.com", birthDate));

        Assert.Equal("Iure Silva", user.Name);
        Assert.Equal("iure.silva@example.com", user.Email);
        Assert.Equal(birthDate, user.Birthdate);
        Assert.Equal(user.Name, response.Name);
        Assert.Equal(user.Email, response.Email);
    }

    [Fact]
    public async Task UpdateCurrentUser_WithTakenEmail_ThrowsApplicationException()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var anotherUser = User.Create("Other", "other@example.com", "hashed-password");
        var useCase = new UpdateCurrentUserUseCase(new FakeUserRepository(user, anotherUser));

        await Assert.ThrowsAsync<ApplicationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                new UpdateCurrentUserRequest("Iure", anotherUser.Email, null)));
    }

    [Fact]
    public async Task ChangeCurrentUserPassword_WithValidCurrentPassword_ChangesPassword()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var useCase = new ChangeCurrentUserPasswordUseCase(
            new FakeUserRepository(user),
            new FakePasswordHasher(true));

        await useCase.ExecuteAsync(
            user.Id,
            new ChangeCurrentUserPasswordRequest("current-password", "new-password"));

        Assert.Equal("hashed-new-password", user.PasswordHash);
    }

    [Fact]
    public async Task ChangeCurrentUserPassword_WithInvalidCurrentPassword_ThrowsUnauthorized()
    {
        var user = User.Create("Iure", "iure@example.com", "hashed-password");
        var useCase = new ChangeCurrentUserPasswordUseCase(
            new FakeUserRepository(user),
            new FakePasswordHasher(false));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                new ChangeCurrentUserPasswordRequest("wrong-password", "new-password")));
    }

    private static void SetProperty<T>(T target, string propertyName, object? value)
    {
        var property = typeof(T).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.NotNull(property);
        property.SetValue(target, value);
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        public List<User> RegisteredUsers { get; } = [];

        public Task<User?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(users.FirstOrDefault(user => user.Id == id));
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return Task.FromResult(users.FirstOrDefault(user => user.Email == email));
        }

        public Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId)
        {
            return Task.FromResult<IReadOnlyCollection<string>>(["player"]);
        }

        public Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId)
        {
            return Task.FromResult<IReadOnlyCollection<string>>(["can_play_games"]);
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            return Task.FromResult(users.Any(user => user.Email == email));
        }

        public Task RegisterPlayerAsync(User user)
        {
            RegisteredUsers.Add(user);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRefreshTokenRepository(params RefreshToken[] tokens) : IRefreshTokenRepository
    {
        public List<RefreshToken> Tokens { get; } = [.. tokens];

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return Task.FromResult(Tokens.FirstOrDefault(token => token.TokenHash == tokenHash));
        }

        public Task<RefreshToken?> RotateAsync(string oldTokenHash, string newTokenHash)
        {
            var oldToken = Tokens.FirstOrDefault(token => token.TokenHash == oldTokenHash);

            if (oldToken is null || !oldToken.IsActive)
            {
                return Task.FromResult<RefreshToken?>(null);
            }

            oldToken.Revoke();
            var newToken = RefreshToken.Create(oldToken.UserId, newTokenHash);
            Tokens.Add(newToken);

            return Task.FromResult<RefreshToken?>(newToken);
        }

        public Task RevokeActiveTokensForUserAsync(Guid userId)
        {
            foreach (var token in Tokens.Where(token => token.UserId == userId && token.IsActive))
            {
                token.Revoke();
            }

            return Task.CompletedTask;
        }

        public Task AddAsync(RefreshToken refreshToken)
        {
            Tokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordResetTokenRepository(params PasswordResetToken[] tokens) : IPasswordResetTokenRepository
    {
        public List<PasswordResetToken> Tokens { get; } = [.. tokens];

        public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
        {
            return Task.FromResult(Tokens.FirstOrDefault(token => token.TokenHash == tokenHash));
        }

        public Task AddAsync(PasswordResetToken passwordResetToken)
        {
            Tokens.Add(passwordResetToken);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher(bool isValid) : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return $"hashed-{password}";
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return isValid;
        }
    }

    private sealed class FakeJwtService : IJwtService
    {
        private int _refreshTokenNumber;

        public string GenerateAccessToken(
            User user,
            IReadOnlyCollection<string> roles,
            IReadOnlyCollection<string> permissions)
        {
            return "access-token";
        }

        public string GenerateRefreshToken()
        {
            _refreshTokenNumber++;
            return $"refresh-token-{_refreshTokenNumber}";
        }

        public string HashRefreshToken(string refreshToken)
        {
            return $"hashed:{refreshToken}";
        }
    }

    private sealed class FakeSecureTokenService : ISecureTokenService
    {
        public string GenerateToken()
        {
            return "password-reset-token";
        }

        public string HashToken(string token)
        {
            return $"hashed:{token}";
        }
    }

    private sealed class FakePasswordResetEmailSender : IPasswordResetEmailSender
    {
        public List<(string Email, string ResetToken)> Messages { get; } = [];

        public Task SendAsync(string email, string resetToken)
        {
            Messages.Add((email, resetToken));
            return Task.CompletedTask;
        }
    }
}
