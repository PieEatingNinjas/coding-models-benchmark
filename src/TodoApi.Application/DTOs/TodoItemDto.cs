using TodoApi.Domain.Entities;
using TodoApi.Domain.Enums;

namespace TodoApi.Application.DTOs;

/// <summary>Public shape of a todo. Deliberately omits <c>Secret</c>.</summary>
public class TodoItemDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTimeOffset? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
}

public static class TodoMapper
{
    public static TodoItemDto ToDto(this TodoItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        IsComplete = item.IsComplete,
        Priority = item.Priority,
        DueDate = item.DueDate,
        Tags = item.Tags?.ToList() ?? []
    };
}
