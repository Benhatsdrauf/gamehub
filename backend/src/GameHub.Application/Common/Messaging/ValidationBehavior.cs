using FluentValidation;
using GameHub.Application.Common.Errors;
using GameHub.Application.Common.Results;

namespace GameHub.Application.Common.Messaging;

// The single home for FluentValidation. It runs before every handler whose request
// has validators; on failure it short-circuits with a failed Result and the handler
// never runs. Replaces the ~10-line validation block that was copied into each handler.
//
// Constraint: TResponse : Result — true for every handler in the app, and it's what
// lets us build a failure result generically below.
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Requests with no validator (e.g. simple queries) just flow through.
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Same shape the API expects: field name -> messages.
        var errors = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return CreateValidationFailure(new ValidationError(errors));
    }

    // Build a FAILED TResponse without knowing its exact shape at compile time.
    private static TResponse CreateValidationFailure(ValidationError error)
    {
        // Non-generic Result (e.g. DeleteUser returns Result).
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        // Otherwise TResponse is Result<TValue> — grab TValue and call the generic
        // Result.Failure<TValue>(error) via reflection.
        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var failureMethod = typeof(Result)
            .GetMethods()
            .First(method => method is { Name: nameof(Result.Failure), IsGenericMethod: true })
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
