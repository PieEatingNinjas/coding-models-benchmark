using TodoApi.Domain.Enums;

namespace TodoApi.Domain.Entities;

/// <summary>
/// Persistence entity for a todo. <see cref="Secret"/> is sensitive and must never
/// be exposed through the API — endpoints return DTOs instead.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTimeOffset? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];

    /// <summary>Internal-only value; intentionally absent from the DTO.</summary>
    public string? Secret { get; set; }
}
