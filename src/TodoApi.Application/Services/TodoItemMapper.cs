using TodoApi.Models;

namespace TodoApi.Models;

public static class TodoItemMapper
{
    public static TodoItemDto ToDto(this TodoItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        IsComplete = item.IsComplete,
        Priority = item.Priority,
        DueDate = item.DueDate,
        Tags = TodoItem.NormalizeTags(item.Tags),
    };
}
