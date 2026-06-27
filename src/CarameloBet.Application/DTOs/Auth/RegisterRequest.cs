namespace CarameloBet.Application.DTOs.Auth;

public record RegisterRequest(
    string Name,
    string Email,
    string Password,
    DateOnly? BirthDate
);

public record RegisterResponse(
    Guid Id,
    string Name,
    string Email,
    DateOnly? BirthDate,
    string Status,
    DateTime CreatedAt
);
