namespace CarameloBet.Application.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password
);

public record AuthenticatedUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    IReadOnlyCollection<string> Roles
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    AuthenticatedUserResponse User
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
);

public record LogoutRequest(
    string RefreshToken
);

public record ForgotPasswordRequest(
    string Email
);

public record ResetPasswordRequest(
    string Token,
    string NewPassword
);
