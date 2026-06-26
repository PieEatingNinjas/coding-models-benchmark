namespace TodoApi.Application.Todos;

public sealed record TodoCreateResult(TodoItemDto? Todo, IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    public static TodoCreateResult Success(TodoItemDto todo) => new(todo, null);
    public static TodoCreateResult Validation(IReadOnlyDictionary<string, string[]> validationErrors) =>
        new(null, validationErrors);
}
