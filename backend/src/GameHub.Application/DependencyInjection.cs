using GameHub.Application.Users.RegisterUser;
using Microsoft.Extensions.DependencyInjection;

namespace GameHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped: the handler depends on IUserRepository, which is scoped.
        // A service must never outlive its dependencies.
        services.AddScoped<RegisterUserHandler>();

        return services;
    }
}
