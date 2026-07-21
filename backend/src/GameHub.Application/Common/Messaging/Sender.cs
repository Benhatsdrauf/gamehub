using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;

namespace GameHub.Application.Common.Messaging;

// The dispatcher. Given a request, it finds the one handler registered for that
// request's concrete type, wraps it in the pipeline behaviors, and runs the chain.
public sealed class Sender : ISender
{
    private readonly IServiceProvider _provider;

    public Sender(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        // The compile-time type is IRequest<TResponse>; the runtime type is the concrete
        // command/query (e.g. RegisterUserCommand). We need the concrete one to find its handler.
        var requestType = request.GetType();

        // Build the closed handler interface, e.g. IRequestHandler<RegisterUserCommand, Result<...>>,
        // and resolve THE single handler registered for it.
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _provider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")!;

        // The innermost link of the chain: actually call the handler.
        RequestHandlerDelegate<TResponse> next = () =>
            Invoke<TResponse>(handleMethod, handler, [request, cancellationToken]);

        // Wrap the handler in each behavior. We reverse so the FIRST-registered behavior
        // ends up OUTERMOST — registration order == execution order.
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviorMethod = behaviorType.GetMethod("Handle")!;

        foreach (var behavior in _provider.GetServices(behaviorType).Reverse())
        {
            var nextLink = next; // capture the current chain as THIS behavior's "next"
            next = () => Invoke<TResponse>(behaviorMethod, behavior!, [request, nextLink, cancellationToken]);
        }

        // Start the chain: outermost behavior → ... → handler.
        return next();
    }

    // MethodInfo.Invoke wraps whatever the target throws in a TargetInvocationException.
    // We unwrap it (preserving the original stack trace) so a caller sees the SAME
    // exception it would have seen calling the handler directly — reflection stays invisible.
    private static Task<TResponse> Invoke<TResponse>(MethodInfo method, object target, object?[] args)
    {
        try
        {
            return (Task<TResponse>)method.Invoke(target, args)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw; // unreachable; satisfies the compiler
        }
    }
}
