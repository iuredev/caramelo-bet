namespace CarameloBet.Application.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password
);

public record AuthenticatedUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Status
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    AuthenticatedUserResponse User
);
