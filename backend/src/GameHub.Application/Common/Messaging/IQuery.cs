namespace GameHub.Application.Common.Messaging;

// A query: a request that READS state without changing it. Semantic marker over IRequest.
public interface IQuery<TResponse> : IRequest<TResponse> { }
