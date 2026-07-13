using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CarameloBet.API.Models;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Application.UseCases.Auth;
using FluentValidation;

namespace CarameloBet.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            GetCurrentUserUseCase useCase) =>
        {
            var userId = GetUserId(principal);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var response = await useCase.ExecuteAsync(userId.Value);

            return Results.Ok(ApiResponse<UserProfileResponse>.Ok(response));
        });

        group.MapPut("/me", async (
            UpdateCurrentUserRequest request,
            ClaimsPrincipal principal,
            UpdateCurrentUserUseCase useCase,
            IValidator<UpdateCurrentUserRequest> validator) =>
        {
            var userId = GetUserId(principal);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray());

                return Results.ValidationProblem(errors);
            }

            var response = await useCase.ExecuteAsync(userId.Value, request);

            return Results.Ok(ApiResponse<UserProfileResponse>.Ok(response));
        });

        group.MapPut("/me/password", async (
            ChangeCurrentUserPasswordRequest request,
            ClaimsPrincipal principal,
            ChangeCurrentUserPasswordUseCase useCase,
            IValidator<ChangeCurrentUserPasswordRequest> validator) =>
        {
            var userId = GetUserId(principal);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray());

                return Results.ValidationProblem(errors);
            }

            await useCase.ExecuteAsync(userId.Value, request);

            return Results.Ok(ApiResponse<string>.Ok("Password changed"));
        });

        return app;
    }

    private static Guid? GetUserId(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userId, out var parsedUserId)
            ? parsedUserId
            : null;
    }
}
