using TodoApi.Domain.Todos;

namespace TodoApi.Application.Todos;

public sealed class TodoService(ITodoRepository repository) : ITodoService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ITodoRepository _repository = repository;

    public async Task<TodoListResult> GetTodosAsync(string? tag, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var resolvedPage = page ?? DefaultPage;
        var resolvedPageSize = pageSize ?? DefaultPageSize;
        var paginationErrors = ValidatePagination(resolvedPage, resolvedPageSize);
        if (paginationErrors.Count > 0)
        {
            return TodoListResult.Validation(paginationErrors);
        }

        if (resolvedPageSize > MaxPageSize)
        {
            resolvedPageSize = MaxPageSize;
        }

        var normalizedTag = string.IsNullOrWhiteSpace(tag) ? null : NormalizeTag(tag);
        var totalCount = await _repository.CountAsync(normalizedTag, cancellationToken);
        var skip = (resolvedPage - 1) * resolvedPageSize;
        var entities = await _repository.GetPageAsync(normalizedTag, skip, resolvedPageSize, cancellationToken);
        var items = entities.Select(t => t.ToDto()).ToList();

        return TodoListResult.Success(items, totalCount);
    }

    public async Task<IReadOnlyList<TodoItemDto>> GetCompleteAsync(CancellationToken cancellationToken) =>
        (await _repository.GetCompleteAsync(cancellationToken)).Select(t => t.ToDto()).ToList();

    public async Task<IReadOnlyList<TodoItemDto>> GetOverdueAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return (await _repository.GetOverdueAsync(now, cancellationToken)).Select(t => t.ToDto()).ToList();
    }

    public async Task<TodoPriorityResult> GetByPriorityAsync(string priority, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TodoPriority>(priority, ignoreCase: true, out var parsedPriority) ||
            !Enum.IsDefined(parsedPriority))
        {
            return TodoPriorityResult.Invalid(
                $"Invalid priority '{priority}'. Expected one of: Low, Medium, High.");
        }

        var entities = await _repository.GetByPriorityAsync(parsedPriority, cancellationToken);
        return TodoPriorityResult.Success(entities.Select(t => t.ToDto()).ToList());
    }

    public async Task<TodoItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        (await _repository.GetByIdAsync(id, cancellationToken))?.ToDto();

    public async Task<TodoCreateResult> CreateAsync(TodoItemDto dto, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateTodoInput(dto);
        if (validationErrors.Count > 0)
        {
            return TodoCreateResult.Validation(validationErrors);
        }

        var created = await _repository.AddAsync(new TodoItem
        {
            Name = dto.Name,
            IsComplete = dto.IsComplete,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            Tags = NormalizeTags(dto.Tags),
        }, cancellationToken);

        return TodoCreateResult.Success(created.ToDto());
    }

    public async Task<TodoUpdateResult> UpdateAsync(int id, TodoItemDto dto, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(id, cancellationToken);
        if (todo is null)
        {
            return TodoUpdateResult.Missing();
        }

        var validationErrors = ValidateTodoInput(dto);
        if (validationErrors.Count > 0)
        {
            return TodoUpdateResult.Validation(validationErrors);
        }

        todo.Name = dto.Name;
        todo.IsComplete = dto.IsComplete;
        todo.Priority = dto.Priority;
        todo.DueDate = dto.DueDate;
        todo.Tags = NormalizeTags(dto.Tags);
        await _repository.SaveChangesAsync(cancellationToken);

        return TodoUpdateResult.Success();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(id, cancellationToken);
        if (todo is null)
        {
            return false;
        }

        _repository.Remove(todo);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool DueDateIsInPast(DateTimeOffset? dueDate) =>
        dueDate is { } value && value < DateTimeOffset.UtcNow;

    private static string NormalizeTag(string tag) => tag.Trim().ToLowerInvariant();

    private static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return [];
        }

        HashSet<string> normalizedTags = [];
        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            normalizedTags.Add(NormalizeTag(tag));
        }

        return [.. normalizedTags];
    }

    private static Dictionary<string, string[]> ValidateTodoInput(TodoItemDto dto)
    {
        Dictionary<string, string[]> errors = [];
        var name = dto.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Name is required."];
        }

        if (name.Length > 200)
        {
            errors["name"] = ["Name must be 200 characters or fewer."];
        }

        if (DueDateIsInPast(dto.DueDate))
        {
            errors["dueDate"] = ["DueDate cannot be in the past."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidatePagination(int page, int pageSize)
    {
        Dictionary<string, string[]> errors = [];
        if (page <= 0)
        {
            errors["page"] = ["Page must be greater than 0."];
        }

        if (pageSize <= 0)
        {
            errors["pageSize"] = ["PageSize must be greater than 0."];
        }

        return errors;
    }
}
