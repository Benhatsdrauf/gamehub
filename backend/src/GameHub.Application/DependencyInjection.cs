using FluentValidation;
using GameHub.Application.Users.DeleteUser;
using GameHub.Application.Users.GetUser;
using GameHub.Application.Users.GetUsers;
using GameHub.Application.Users.RegisterUser;
using GameHub.Application.Users.UpdateUser;
using Microsoft.Extensions.DependencyInjection;

namespace GameHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped: the handler depends on IUserRepository, which is scoped.
        // A service must never outlive its dependencies.
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<DeleteUserHandler>();

        // Registers every AbstractValidator in this assembly (e.g.
        // RegisterUserCommandValidator) as IValidator<T> for injection.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
