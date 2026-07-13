using CarameloBet.Application.Abstractions;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Domain.Entities.Auth;

namespace CarameloBet.Application.UseCases.Auth;

public class ListRolesUseCase(IRoleRepository roles)
{
    public async Task<IReadOnlyCollection<RoleResponse>> ExecuteAsync()
    {
        var allRoles = await roles.ListAsync();

        return allRoles
            .Select(Map)
            .ToArray();
    }

    private static RoleResponse Map(Role role)
    {
        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.CreatedAt);
    }
}

public class CreateRoleUseCase(IRoleRepository roles)
{
    public async Task<RoleResponse> ExecuteAsync(CreateRoleRequest request)
    {
        if (await roles.NameExistsAsync(request.Name))
        {
            throw new ApplicationException("Role already exists");
        }

        var role = Role.Create(request.Name, request.Description);
        await roles.AddAsync(role);
        await roles.SaveChangesAsync();

        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.CreatedAt);
    }
}

public class UpdateRoleUseCase(IRoleRepository roles)
{
    public async Task<RoleResponse> ExecuteAsync(Guid roleId, UpdateRoleRequest request)
    {
        var role = await roles.GetByIdAsync(roleId)
            ?? throw new KeyNotFoundException("Role not found");

        if (!string.Equals(role.Name, request.Name, StringComparison.OrdinalIgnoreCase)
            && await roles.NameExistsForOtherRoleAsync(role.Id, request.Name))
        {
            throw new ApplicationException("Role already exists");
        }

        role.Update(request.Name, request.Description);
        await roles.SaveChangesAsync();

        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.CreatedAt);
    }
}

public class AssignUserRoleUseCase(
    IAdminUserRepository users,
    IRoleRepository roles)
{
    public async Task ExecuteAsync(Guid userId, AssignRoleRequest request)
    {
        _ = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        _ = await roles.GetByIdAsync(request.RoleId)
            ?? throw new KeyNotFoundException("Role not found");

        if (await roles.UserHasRoleAsync(userId, request.RoleId))
        {
            return;
        }

        await roles.AssignRoleAsync(userId, request.RoleId);
        await roles.SaveChangesAsync();
    }
}
