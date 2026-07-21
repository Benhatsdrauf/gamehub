namespace GameHub.Application.Common.Messaging;

// The dispatcher. A controller hands it a request and gets back the response, without
// knowing which handler runs. The TResponse is inferred from the request's IRequest<T>.
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
