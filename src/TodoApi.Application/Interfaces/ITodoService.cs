using TodoApi.Application.DTOs;
using TodoApi.Domain.Enums;

namespace TodoApi.Application.Interfaces;

public interface ITodoService
{
    Task<(IEnumerable<TodoItemDto> Items, int TotalCount)> GetPaginatedAsync(string? tag, int page, int pageSize);
    Task<IEnumerable<TodoItemDto>> GetCompleteAsync();
    Task<IEnumerable<TodoItemDto>> GetByPriorityAsync(Priority priority);
    Task<IEnumerable<TodoItemDto>> GetOverdueAsync();
    Task<TodoItemDto?> GetByIdAsync(int id);
    Task<TodoItemDto> CreateAsync(TodoItemDto dto);
    Task<bool> UpdateAsync(int id, TodoItemDto dto);
    Task<bool> DeleteAsync(int id);
}
