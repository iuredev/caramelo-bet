using CarameloBet.Application.UseCases.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace CarameloBet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUseCase>();
        services.AddScoped<LoginUseCase>();


        return services;
    }
}
