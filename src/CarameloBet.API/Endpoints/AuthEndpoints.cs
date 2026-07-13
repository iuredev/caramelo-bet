using CarameloBet.API.Models;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Application.UseCases.Auth;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;

namespace CarameloBet.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .RequireRateLimiting("auth");

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

        group.MapPost("/refresh", async (
            RefreshTokenRequest request,
            RefreshTokenUseCase useCase,
            IValidator<RefreshTokenRequest> validator) =>
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

            return Results.Ok(ApiResponse<RefreshTokenResponse>.Ok(response));
        })
        .AllowAnonymous();

        group.MapPost("/logout", async (
            LogoutRequest request,
            LogoutUseCase useCase,
            IValidator<LogoutRequest> validator) =>
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

            await useCase.ExecuteAsync(request);

            return Results.Ok(ApiResponse<string>.Ok("Logout completed"));
        })
        .AllowAnonymous();

        group.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            ForgotPasswordUseCase useCase,
            IValidator<ForgotPasswordRequest> validator) =>
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

            await useCase.ExecuteAsync(request);

            return Results.Ok(ApiResponse<string>.Ok("If the email exists, a reset link will be sent"));
        })
        .AllowAnonymous();

        group.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            ResetPasswordUseCase useCase,
            IValidator<ResetPasswordRequest> validator) =>
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

            await useCase.ExecuteAsync(request);

            return Results.Ok(ApiResponse<string>.Ok("Password reset completed"));
        })
        .AllowAnonymous();

        return app;
    }
}
