namespace TodoApi.Models;

/// <summary>
/// Persistence entity for a todo. <see cref="Secret"/> is sensitive and must never
/// be exposed through the API — endpoints return <see cref="TodoItemDto"/> instead.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    /// <summary>Internal-only value; intentionally absent from the DTO.</summary>
    public string? Secret { get; set; }
}
