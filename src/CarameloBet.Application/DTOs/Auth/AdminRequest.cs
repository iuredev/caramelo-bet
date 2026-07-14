namespace CarameloBet.Application.DTOs.Auth;

public record AdminUserResponse(
    Guid Id,
    string Name,
    string Email,
    DateOnly? BirthDate,
    string Status,
    string? BlockedReason,
    DateTime? BlockedUntil,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<string> Roles
);

public record UpdateAdminUserRequest(
    string Name,
    string Email,
    DateOnly? BirthDate
);

public record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt
);

public record CreateRoleRequest(
    string Name,
    string Description
);

public record UpdateRoleRequest(
    string Name,
    string Description
);

public record AssignRoleRequest(
    Guid RoleId
);

public record BlockUserRequest(
    string Reason,
    DateTimeOffset? ExpiresAt
);
