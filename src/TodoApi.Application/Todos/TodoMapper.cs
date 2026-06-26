using TodoApi.Domain.Todos;

namespace TodoApi.Application.Todos;

public static class TodoMapper
{
    public static TodoItemDto ToDto(this TodoItem item) => new()
    {
        Id = item.Id,
        Name = item.Name ?? string.Empty,
        IsComplete = item.IsComplete,
        Priority = item.Priority,
        DueDate = item.DueDate,
        Tags = item.Tags is { Count: > 0 } tags ? [.. tags] : [],
    };
}
