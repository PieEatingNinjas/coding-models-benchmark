namespace TodoApi.Domain.Todos;

/// <summary>
/// Domain/persistence entity for a todo. <see cref="Secret"/> is sensitive and
/// must never be exposed through API responses.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public DateTimeOffset? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? Secret { get; set; }
}
