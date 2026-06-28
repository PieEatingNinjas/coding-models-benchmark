using TodoApi.Domain.Entities;
using TodoApi.Domain.Enums;

namespace TodoApi.Application.Interfaces;

public interface ITodoRepository
{
    Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetPaginatedAsync(string? tag, int page, int pageSize);
    Task<IEnumerable<TodoItem>> GetCompleteAsync();
    Task<IEnumerable<TodoItem>> GetByPriorityAsync(Priority priority);
    Task<IEnumerable<TodoItem>> GetOverdueAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task AddAsync(TodoItem todo);
    Task UpdateAsync(TodoItem todo);
    Task DeleteAsync(TodoItem todo);
}
