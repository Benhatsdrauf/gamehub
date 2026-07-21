namespace GameHub.Application.Common.Messaging;

// "The next step in the chain" — either the next behavior or, at the end, the handler.
// Calling it runs the rest of the pipeline and yields the eventual TResponse.
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

// A wrapper around request handling. Each behavior can run code before calling next()
// (e.g. validate, start a timer) and after it returns (e.g. log the result). This is
// the single seam where cross-cutting concerns live instead of being copied per handler.
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
