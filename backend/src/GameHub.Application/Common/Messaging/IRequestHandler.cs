namespace GameHub.Application.Common.Messaging;

// Handles exactly one request type and returns its TResponse. This is the base the
// dispatcher (ISender) resolves and invokes. ICommandHandler/IQueryHandler below are
// thin aliases so a handler declares which side of CQRS it is on.
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
