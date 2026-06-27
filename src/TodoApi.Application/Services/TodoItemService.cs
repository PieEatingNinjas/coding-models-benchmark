using TodoApi.Application.Abstractions;
using TodoApi.Application.Exceptions;
using TodoApi.Application.Models;
using TodoApi.Models;

namespace TodoApi.Application.Services;

public sealed class TodoItemService(ITodoItemRepository repository)
{
    public async Task<TodoQueryResult> GetItemsAsync(string? tag, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedTag = TodoItem.NormalizeTag(tag);
        var allItems = await repository.GetAllAsync(cancellationToken);
        var filteredItems = allItems
            .Where(todo => string.IsNullOrEmpty(normalizedTag) || todo.HasTag(normalizedTag))
            .ToList();

        var requestedPage = page ?? 1;
        var requestedPageSize = pageSize ?? 20;
        var effectivePageSize = Math.Min(requestedPageSize, 100);
        var skip = (requestedPage - 1) * effectivePageSize;

        var items = filteredItems
            .OrderBy(todo => todo.Id)
            .Skip(skip)
            .Take(effectivePageSize)
            .Select(todo => todo.ToDto())
            .ToList();

        return new TodoQueryResult(items, filteredItems.Count);
    }

    public async Task<IReadOnlyList<TodoItemDto>> GetCompleteAsync(CancellationToken cancellationToken = default)
        => (await repository.GetCompletedAsync(cancellationToken)).Select(todo => todo.ToDto()).ToList();

    public async Task<IReadOnlyList<TodoItemDto>> GetOverdueAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        => (await repository.GetOverdueAsync(now, cancellationToken)).Select(todo => todo.ToDto()).ToList();

    public async Task<IReadOnlyList<TodoItemDto>> GetByPriorityAsync(TodoPriority priority, CancellationToken cancellationToken = default)
        => (await repository.GetByPriorityAsync(priority, cancellationToken)).Select(todo => todo.ToDto()).ToList();

    public async Task<TodoItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var todo = await repository.GetByIdAsync(id, cancellationToken);
        return todo?.ToDto();
    }

    public async Task<TodoItemDto> CreateAsync(TodoItemDto dto, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        EnsureValidName(dto.Name);
        EnsureValidDueDate(dto.DueDate, now);

        var todo = new TodoItem
        {
            Name = dto.Name,
            IsComplete = dto.IsComplete,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
        };

        todo.SetTags(dto.Tags);
        var created = await repository.AddAsync(todo, cancellationToken);
        return created.ToDto();
    }

    public async Task<TodoItemDto?> UpdateAsync(int id, TodoItemDto dto, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        EnsureValidName(dto.Name);
        EnsureValidDueDate(dto.DueDate, now);

        existing.Name = dto.Name;
        existing.IsComplete = dto.IsComplete;
        existing.Priority = dto.Priority;
        existing.DueDate = dto.DueDate;
        existing.SetTags(dto.Tags);

        await repository.UpdateAsync(existing, cancellationToken);
        return existing.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await repository.DeleteAsync(existing, cancellationToken);
        return true;
    }

    private static void EnsureValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new TodoValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Name is required."],
            });
        }

        if (name.Length > 200)
        {
            throw new TodoValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Name must be 200 characters or fewer."],
            });
        }
    }

    private static void EnsureValidDueDate(DateTimeOffset? dueDate, DateTimeOffset now)
    {
        if (dueDate is { } date && date < now)
        {
            throw new TodoDomainException(
                "Invalid due date",
                "Due date cannot be in the past.",
                400);
        }
    }
}
