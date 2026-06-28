using TodoApi.Application.DTOs;
using TodoApi.Application.Interfaces;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Enums;

namespace TodoApi.Application.Services;

public class TodoService(ITodoRepository repository) : ITodoService
{
    public async Task<(IEnumerable<TodoItemDto> Items, int TotalCount)> GetPaginatedAsync(string? tag, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await repository.GetPaginatedAsync(tag, page, pageSize);
        return (items.Select(t => t.ToDto()), totalCount);
    }

    public async Task<IEnumerable<TodoItemDto>> GetCompleteAsync()
    {
        var items = await repository.GetCompleteAsync();
        return items.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TodoItemDto>> GetByPriorityAsync(Priority priority)
    {
        var items = await repository.GetByPriorityAsync(priority);
        return items.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TodoItemDto>> GetOverdueAsync()
    {
        var items = await repository.GetOverdueAsync();
        return items.Select(t => t.ToDto());
    }

    public async Task<TodoItemDto?> GetByIdAsync(int id)
    {
        var item = await repository.GetByIdAsync(id);
        return item?.ToDto();
    }

    public async Task<TodoItemDto> CreateAsync(TodoItemDto dto)
    {
        var tags = dto.Tags?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        var todo = new TodoItem
        {
            Name = dto.Name,
            IsComplete = dto.IsComplete,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            Tags = tags
        };

        await repository.AddAsync(todo);
        return todo.ToDto();
    }

    public async Task<bool> UpdateAsync(int id, TodoItemDto dto)
    {
        var todo = await repository.GetByIdAsync(id);
        if (todo is null) return false;

        todo.Name = dto.Name;
        todo.IsComplete = dto.IsComplete;
        todo.Priority = dto.Priority;
        todo.DueDate = dto.DueDate;
        todo.Tags = dto.Tags?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];

        await repository.UpdateAsync(todo);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todo = await repository.GetByIdAsync(id);
        if (todo is null) return false;

        await repository.DeleteAsync(todo);
        return true;
    }
}
