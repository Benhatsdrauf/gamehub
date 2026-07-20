namespace GameHub.Application.Common.Errors;

// A specialised Error that carries per-field validation messages. It IS an
// Error (Type = Validation), so it flows through Result like any other failure;
// the extra Errors map lets the API emit a standard ValidationProblemDetails.
public sealed record ValidationError : Error
{
    public ValidationError(IReadOnlyDictionary<string, string[]> errors)
        : base(
            "Validation.Failed",
            "One or more validation errors occurred.",
            ErrorType.Validation)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
