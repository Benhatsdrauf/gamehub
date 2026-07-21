namespace GameHub.Application.Common.Messaging;

// Handles a command. Inherits the real contract from IRequestHandler; exists only
// so a write handler reads as "ICommandHandler<...>" at a glance.
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
