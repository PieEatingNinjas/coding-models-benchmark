namespace TodoApi.Application.Todos;

public sealed record TodoPriorityResult(IReadOnlyList<TodoItemDto> Items, string? ErrorMessage)
{
    public static TodoPriorityResult Success(IReadOnlyList<TodoItemDto> items) =>
        new(items, null);

    public static TodoPriorityResult Invalid(string errorMessage) =>
        new([], errorMessage);
}
