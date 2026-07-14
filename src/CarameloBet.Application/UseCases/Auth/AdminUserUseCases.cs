using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class ListAdminUsersUseCase(IAdminUserRepository users)
{
    public async Task<IReadOnlyCollection<AdminUserResponse>> ExecuteAsync()
    {
        var allUsers = await users.ListAsync();
        var responses = new List<AdminUserResponse>();

        foreach (var user in allUsers)
        {
            responses.Add(await MapAsync(users, user));
        }

        return responses;
    }

    private static async Task<AdminUserResponse> MapAsync(IAdminUserRepository users, User user)
    {
        var roles = await users.GetRolesAsync(user.Id);

        return new AdminUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Birthdate,
            user.Status,
            user.BlockedReason,
            user.BlockedUntil,
            user.CreatedAt,
            user.UpdatedAt,
            roles);
    }
}

public class GetAdminUserUseCase(IAdminUserRepository users)
{
    public async Task<AdminUserResponse> ExecuteAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var roles = await users.GetRolesAsync(user.Id);

        return new AdminUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Birthdate,
            user.Status,
            user.BlockedReason,
            user.BlockedUntil,
            user.CreatedAt,
            user.UpdatedAt,
            roles);
    }
}

public class UpdateAdminUserUseCase(IAdminUserRepository users)
{
    public async Task<AdminUserResponse> ExecuteAsync(Guid userId, UpdateAdminUserRequest request)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && await users.EmailExistsForOtherUserAsync(user.Id, request.Email))
        {
            throw new ApplicationException("Email already exists");
        }

        user.UpdateProfile(request.Name, request.Email, request.BirthDate);
        await users.SaveChangesAsync();

        var roles = await users.GetRolesAsync(user.Id);

        return new AdminUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Birthdate,
            user.Status,
            user.BlockedReason,
            user.BlockedUntil,
            user.CreatedAt,
            user.UpdatedAt,
            roles);
    }
}

public class DeleteAdminUserUseCase(IAdminUserRepository users)
{
    public async Task ExecuteAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        user.UpdateStatus("deleted");
        await users.SaveChangesAsync();
    }
}

public class BlockAdminUserUseCase(
    IAdminUserRepository users,
    IRefreshTokenRepository refreshTokens)
{
    public async Task<AdminUserResponse> ExecuteAsync(Guid userId, BlockUserRequest request)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        user.Block(request.Reason, request.ExpiresAt?.UtcDateTime);
        await refreshTokens.RevokeActiveTokensForUserAsync(user.Id);
        await users.SaveChangesAsync();
        await refreshTokens.SaveChangesAsync();

        return await MapAsync(users, user);
    }

    private static async Task<AdminUserResponse> MapAsync(IAdminUserRepository users, User user)
    {
        var roles = await users.GetRolesAsync(user.Id);

        return new AdminUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Birthdate,
            user.Status,
            user.BlockedReason,
            user.BlockedUntil,
            user.CreatedAt,
            user.UpdatedAt,
            roles);
    }
}

public class UnblockAdminUserUseCase(IAdminUserRepository users)
{
    public async Task<AdminUserResponse> ExecuteAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        user.Unblock();
        await users.SaveChangesAsync();

        var roles = await users.GetRolesAsync(user.Id);

        return new AdminUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Birthdate,
            user.Status,
            user.BlockedReason,
            user.BlockedUntil,
            user.CreatedAt,
            user.UpdatedAt,
            roles);
    }
}
