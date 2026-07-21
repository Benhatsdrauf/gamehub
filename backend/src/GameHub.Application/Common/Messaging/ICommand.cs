namespace GameHub.Application.Common.Messaging;

// A command: a request that CHANGES state (a write). Purely a semantic marker over
// IRequest — same CQRS split as IUserRepository (writes) vs IUserQueries (reads).
public interface ICommand<TResponse> : IRequest<TResponse> { }
