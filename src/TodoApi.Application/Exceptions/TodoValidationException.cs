namespace TodoApi.Application.Exceptions;

public sealed class TodoValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public TodoValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }
}
