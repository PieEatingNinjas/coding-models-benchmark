using System.ComponentModel.DataAnnotations;
using TodoApi.Domain.Todos;

namespace TodoApi.Application.Todos;

/// <summary>Public shape of a todo. Deliberately omits <c>Secret</c>.</summary>
public class TodoItemDto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsComplete { get; set; }
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public DateTimeOffset? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
}
