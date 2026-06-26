using TodoApi.Application.Todos;
using TodoApi.Domain.Todos;

namespace TodoApi.Tests.Unit.Support;

public sealed class MockTodoRepository : ITodoRepository
{
    public Func<string?, CancellationToken, Task<int>> CountAsyncHandler { get; set; } =
        (_, _) => Task.FromResult(0);

    public Func<string?, int, int, CancellationToken, Task<IReadOnlyList<TodoItem>>> GetPageAsyncHandler { get; set; } =
        (_, _, _, _) => Task.FromResult<IReadOnlyList<TodoItem>>([]);

    public Func<CancellationToken, Task<IReadOnlyList<TodoItem>>> GetCompleteAsyncHandler { get; set; } =
        _ => Task.FromResult<IReadOnlyList<TodoItem>>([]);

    public Func<DateTimeOffset, CancellationToken, Task<IReadOnlyList<TodoItem>>> GetOverdueAsyncHandler { get; set; } =
        (_, _) => Task.FromResult<IReadOnlyList<TodoItem>>([]);

    public Func<TodoPriority, CancellationToken, Task<IReadOnlyList<TodoItem>>> GetByPriorityAsyncHandler { get; set; } =
        (_, _) => Task.FromResult<IReadOnlyList<TodoItem>>([]);

    public Func<int, CancellationToken, Task<TodoItem?>> GetByIdAsyncHandler { get; set; } =
        (_, _) => Task.FromResult<TodoItem?>(null);

    public Func<TodoItem, CancellationToken, Task<TodoItem>> AddAsyncHandler { get; set; } =
        (todo, _) => Task.FromResult(todo);

    public Func<CancellationToken, Task> SaveChangesAsyncHandler { get; set; } =
        _ => Task.CompletedTask;

    public Action<TodoItem> RemoveHandler { get; set; } = _ => { };

    public Task<int> CountAsync(string? normalizedTag, CancellationToken cancellationToken) =>
        CountAsyncHandler(normalizedTag, cancellationToken);

    public Task<IReadOnlyList<TodoItem>> GetPageAsync(string? normalizedTag, int skip, int take, CancellationToken cancellationToken) =>
        GetPageAsyncHandler(normalizedTag, skip, take, cancellationToken);

    public Task<IReadOnlyList<TodoItem>> GetCompleteAsync(CancellationToken cancellationToken) =>
        GetCompleteAsyncHandler(cancellationToken);

    public Task<IReadOnlyList<TodoItem>> GetOverdueAsync(DateTimeOffset now, CancellationToken cancellationToken) =>
        GetOverdueAsyncHandler(now, cancellationToken);

    public Task<IReadOnlyList<TodoItem>> GetByPriorityAsync(TodoPriority priority, CancellationToken cancellationToken) =>
        GetByPriorityAsyncHandler(priority, cancellationToken);

    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        GetByIdAsyncHandler(id, cancellationToken);

    public Task<TodoItem> AddAsync(TodoItem todo, CancellationToken cancellationToken) =>
        AddAsyncHandler(todo, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsyncHandler(cancellationToken);

    public void Remove(TodoItem todo) => RemoveHandler(todo);
}
