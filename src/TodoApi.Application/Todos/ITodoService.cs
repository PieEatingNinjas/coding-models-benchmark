namespace TodoApi.Application.Todos;

public interface ITodoService
{
    Task<TodoListResult> GetTodosAsync(string? tag, int? page, int? pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<TodoItemDto>> GetCompleteAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TodoItemDto>> GetOverdueAsync(CancellationToken cancellationToken);
    Task<TodoPriorityResult> GetByPriorityAsync(string priority, CancellationToken cancellationToken);
    Task<TodoItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TodoCreateResult> CreateAsync(TodoItemDto dto, CancellationToken cancellationToken);
    Task<TodoUpdateResult> UpdateAsync(int id, TodoItemDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
