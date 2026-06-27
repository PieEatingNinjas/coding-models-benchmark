using TodoApi.Models;

namespace TodoApi.Application.Abstractions;

public interface ITodoItemRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> GetOverdueAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> GetByPriorityAsync(TodoPriority priority, CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default);
}
