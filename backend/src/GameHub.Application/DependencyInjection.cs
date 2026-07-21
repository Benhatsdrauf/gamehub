using FluentValidation;
using GameHub.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace GameHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registers every AbstractValidator in this assembly (e.g.
        // RegisterUserCommandValidator) as IValidator<T>. Consumed by ValidationBehavior.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // The mediator dispatcher. Scoped so the IServiceProvider it captures is the
        // request scope — a singleton would capture the root provider and fail to
        // resolve scoped handlers (captive dependency).
        services.AddScoped<ISender, Sender>();

        // Pipeline behaviors, in execution order (first registered = outermost).
        // Validation runs before every handler; more behaviors (logging, timing) slot in here.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Auto-register every IRequestHandler<,> in this assembly, so adding a handler
        // never means editing this file again.
        services.AddRequestHandlers();

        return services;
    }

    // Scans the Application assembly for concrete classes that implement
    // IRequestHandler<TRequest, TResponse> and registers each under its closed
    // interface, e.g. IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>.
    private static void AddRequestHandlers(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var handlerInterface in handlerInterfaces)
                services.AddScoped(handlerInterface, type);
        }
    }
}
