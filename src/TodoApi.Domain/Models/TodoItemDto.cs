namespace TodoApi.Models;

public class TodoItemDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public DateTimeOffset? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
}
