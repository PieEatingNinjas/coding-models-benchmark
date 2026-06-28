using Microsoft.EntityFrameworkCore;
using TodoApi.Application.Interfaces;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Enums;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Infrastructure.Repositories;

public class TodoRepository(TodoDb db) : ITodoRepository
{
    public async Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetPaginatedAsync(string? tag, int page, int pageSize)
    {
        var todos = await db.Todos.ToListAsync();
        var query = todos.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(t => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }

        var totalCount = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, totalCount);
    }

    public async Task<IEnumerable<TodoItem>> GetCompleteAsync()
    {
        return await db.Todos.Where(t => t.IsComplete).ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetByPriorityAsync(Priority priority)
    {
        return await db.Todos.Where(t => t.Priority == priority).ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetOverdueAsync()
    {
        var now = DateTimeOffset.UtcNow;
        return await db.Todos
            .Where(t => !t.IsComplete && t.DueDate.HasValue && t.DueDate < now)
            .ToListAsync();
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await db.Todos.FindAsync(id);
    }

    public async Task AddAsync(TodoItem todo)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TodoItem todo)
    {
        db.Todos.Update(todo);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TodoItem todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
    }
}
