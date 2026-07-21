namespace GameHub.Application.Common.Messaging;

// Marker interface: a message whose handling produces a TResponse. Both commands
// and queries are requests — ICommand/IQuery add intent on top of this.
// The TResponse type parameter is what lets ISender.Send know the return type.
public interface IRequest<TResponse> { }
