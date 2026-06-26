using TodoApi.Domain.Todos;

namespace TodoApi.Application.Todos;

public interface ITodoRepository
{
    Task<int> CountAsync(string? normalizedTag, CancellationToken cancellationToken);
    Task<IReadOnlyList<TodoItem>> GetPageAsync(string? normalizedTag, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyList<TodoItem>> GetCompleteAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TodoItem>> GetOverdueAsync(DateTimeOffset now, CancellationToken cancellationToken);
    Task<IReadOnlyList<TodoItem>> GetByPriorityAsync(TodoPriority priority, CancellationToken cancellationToken);
    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TodoItem> AddAsync(TodoItem todo, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    void Remove(TodoItem todo);
}
