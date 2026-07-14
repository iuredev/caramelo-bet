using CarameloBet.Application.UseCases.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace CarameloBet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<RefreshTokenUseCase>();
        services.AddScoped<LogoutUseCase>();
        services.AddScoped<ForgotPasswordUseCase>();
        services.AddScoped<ResetPasswordUseCase>();
        services.AddScoped<GetCurrentUserUseCase>();
        services.AddScoped<UpdateCurrentUserUseCase>();
        services.AddScoped<ChangeCurrentUserPasswordUseCase>();
        services.AddScoped<ListAdminUsersUseCase>();
        services.AddScoped<GetAdminUserUseCase>();
        services.AddScoped<UpdateAdminUserUseCase>();
        services.AddScoped<DeleteAdminUserUseCase>();
        services.AddScoped<BlockAdminUserUseCase>();
        services.AddScoped<UnblockAdminUserUseCase>();
        services.AddScoped<ListRolesUseCase>();
        services.AddScoped<CreateRoleUseCase>();
        services.AddScoped<UpdateRoleUseCase>();
        services.AddScoped<AssignUserRoleUseCase>();


        return services;
    }
}
