using CarameloBet.API.Models;
using CarameloBet.Application.DTOs.Auth;
using CarameloBet.Application.UseCases.Auth;
using FluentValidation;
using FluentValidation.Results;

namespace CarameloBet.API.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/users", async (ListAdminUsersUseCase useCase) =>
        {
            var response = await useCase.ExecuteAsync();

            return Results.Ok(ApiResponse<IReadOnlyCollection<AdminUserResponse>>.Ok(response));
        });

        group.MapGet("/users/{id:guid}", async (
            Guid id,
            GetAdminUserUseCase useCase) =>
        {
            var response = await useCase.ExecuteAsync(id);

            return Results.Ok(ApiResponse<AdminUserResponse>.Ok(response));
        });

        group.MapPut("/users/{id:guid}", async (
            Guid id,
            UpdateAdminUserRequest request,
            UpdateAdminUserUseCase useCase,
            IValidator<UpdateAdminUserRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ToValidationProblem(validationResult);
            }

            var response = await useCase.ExecuteAsync(id, request);

            return Results.Ok(ApiResponse<AdminUserResponse>.Ok(response));
        });

        group.MapDelete("/users/{id:guid}", async (
            Guid id,
            DeleteAdminUserUseCase useCase) =>
        {
            await useCase.ExecuteAsync(id);

            return Results.Ok(ApiResponse<string>.Ok("User deleted"));
        });

        group.MapPost("/users/{id:guid}/block", async (
            Guid id,
            BlockUserRequest request,
            BlockAdminUserUseCase useCase,
            IValidator<BlockUserRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ToValidationProblem(validationResult);
            }

            var response = await useCase.ExecuteAsync(id, request);

            return Results.Ok(ApiResponse<AdminUserResponse>.Ok(response));
        });

        group.MapDelete("/users/{id:guid}/block", async (
            Guid id,
            UnblockAdminUserUseCase useCase) =>
        {
            var response = await useCase.ExecuteAsync(id);

            return Results.Ok(ApiResponse<AdminUserResponse>.Ok(response));
        });

        group.MapGet("/roles", async (ListRolesUseCase useCase) =>
        {
            var response = await useCase.ExecuteAsync();

            return Results.Ok(ApiResponse<IReadOnlyCollection<RoleResponse>>.Ok(response));
        });

        group.MapPost("/roles", async (
            CreateRoleRequest request,
            CreateRoleUseCase useCase,
            IValidator<CreateRoleRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ToValidationProblem(validationResult);
            }

            var response = await useCase.ExecuteAsync(request);

            return Results.Created(
                $"/api/admin/roles/{response.Id}",
                ApiResponse<RoleResponse>.Ok(response));
        });

        group.MapPut("/roles/{id:guid}", async (
            Guid id,
            UpdateRoleRequest request,
            UpdateRoleUseCase useCase,
            IValidator<UpdateRoleRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ToValidationProblem(validationResult);
            }

            var response = await useCase.ExecuteAsync(id, request);

            return Results.Ok(ApiResponse<RoleResponse>.Ok(response));
        });

        group.MapPost("/users/{userId:guid}/roles", async (
            Guid userId,
            AssignRoleRequest request,
            AssignUserRoleUseCase useCase,
            IValidator<AssignRoleRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return ToValidationProblem(validationResult);
            }

            await useCase.ExecuteAsync(userId, request);

            return Results.Ok(ApiResponse<string>.Ok("Role assigned"));
        });

        return app;
    }

    private static IResult ToValidationProblem(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}
