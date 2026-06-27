using CarameloBet.API.Models;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Application.UseCases.Auth;
using FluentValidation;

namespace CarameloBet.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            RegisterUseCase useCase,
            IValidator<RegisterRequest> validator) =>
        {

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

            var response = await useCase.ExecuteAsync(request);

            return Results.Created(
                $"/auth/users/{response.Id}",
                ApiResponse<RegisterResponse>.Ok(response));
        })
        .AllowAnonymous();

        group.MapPost("/login", async (
            LoginRequest request,
            LoginUseCase useCase,
            IValidator<LoginRequest> validator) =>
        {

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

            var response = await useCase.ExecuteAsync(request);

            return Results.Ok(ApiResponse<LoginResponse>.Ok(response));
        })
        .AllowAnonymous();

        return app;
    }
}
