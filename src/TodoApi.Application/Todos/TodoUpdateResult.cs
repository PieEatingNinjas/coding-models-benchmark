namespace TodoApi.Application.Todos;

public sealed record TodoUpdateResult(bool NotFound, IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    public static TodoUpdateResult Success() => new(false, null);
    public static TodoUpdateResult Missing() => new(true, null);
    public static TodoUpdateResult Validation(IReadOnlyDictionary<string, string[]> validationErrors) =>
        new(false, validationErrors);
}
