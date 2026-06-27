using Microsoft.EntityFrameworkCore;
using TodoApi.Application.Abstractions;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Infrastructure.Repositories;

public sealed class TodoItemRepository(TodoDb db) : ITodoItemRepository
{
    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Todos.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TodoItem>> GetCompletedAsync(CancellationToken cancellationToken = default)
        => await db.Todos.Where(todo => todo.IsComplete).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TodoItem>> GetOverdueAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        => await db.Todos
            .Where(todo => !todo.IsComplete && todo.DueDate.HasValue && todo.DueDate.Value < now)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TodoItem>> GetByPriorityAsync(TodoPriority priority, CancellationToken cancellationToken = default)
        => await db.Todos.Where(todo => todo.Priority == priority).ToListAsync(cancellationToken);

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Todos.SingleOrDefaultAsync(todo => todo.Id == id, cancellationToken);

    public async Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        db.Todos.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
        => await db.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        db.Todos.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
    }
}
