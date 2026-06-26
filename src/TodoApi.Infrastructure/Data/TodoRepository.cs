using Microsoft.EntityFrameworkCore;
using TodoApi.Application.Todos;
using TodoApi.Domain.Todos;

namespace TodoApi.Infrastructure.Data;

public sealed class TodoRepository(TodoDb db) : ITodoRepository
{
    private readonly TodoDb _db = db;

    public async Task<int> CountAsync(string? normalizedTag, CancellationToken cancellationToken)
    {
        var query = QueryByTag(normalizedTag);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetPageAsync(
        string? normalizedTag,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = QueryByTag(normalizedTag);
        return await query.OrderBy(t => t.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetCompleteAsync(CancellationToken cancellationToken) =>
        await _db.Todos.Where(t => t.IsComplete).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TodoItem>> GetOverdueAsync(DateTimeOffset now, CancellationToken cancellationToken) =>
        await _db.Todos
            .Where(t => !t.IsComplete && t.DueDate.HasValue && t.DueDate.Value < now)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TodoItem>> GetByPriorityAsync(TodoPriority priority, CancellationToken cancellationToken) =>
        await _db.Todos.Where(t => t.Priority == priority).ToListAsync(cancellationToken);

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await _db.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TodoItem> AddAsync(TodoItem todo, CancellationToken cancellationToken)
    {
        _db.Todos.Add(todo);
        await _db.SaveChangesAsync(cancellationToken);
        return todo;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _db.SaveChangesAsync(cancellationToken);

    public void Remove(TodoItem todo) => _db.Todos.Remove(todo);

    private IQueryable<TodoItem> QueryByTag(string? normalizedTag)
    {
        var query = _db.Todos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(normalizedTag))
        {
            query = query.Where(t => t.Tags.Contains(normalizedTag));
        }

        return query;
    }
}
