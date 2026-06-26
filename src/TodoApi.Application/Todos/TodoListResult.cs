namespace TodoApi.Application.Todos;

public sealed record TodoListResult(
    IReadOnlyList<TodoItemDto> Items,
    int TotalCount,
    IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    public static TodoListResult Success(IReadOnlyList<TodoItemDto> items, int totalCount) =>
        new(items, totalCount, null);

    public static TodoListResult Validation(IReadOnlyDictionary<string, string[]> validationErrors) =>
        new([], 0, validationErrors);
}
