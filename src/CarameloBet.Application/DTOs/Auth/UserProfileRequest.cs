namespace CarameloBet.Application.DTOs.Auth;

public record UserProfileResponse(
    Guid Id,
    string Name,
    string Email,
    DateOnly? BirthDate,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UpdateCurrentUserRequest(
    string Name,
    string Email,
    DateOnly? BirthDate
);

public record ChangeCurrentUserPasswordRequest(
    string CurrentPassword,
    string NewPassword
);
