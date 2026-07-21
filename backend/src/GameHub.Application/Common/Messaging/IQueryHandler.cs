namespace GameHub.Application.Common.Messaging;

// Handles a query. Inherits the real contract from IRequestHandler; exists only so a
// read handler reads as "IQueryHandler<...>" at a glance.
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
