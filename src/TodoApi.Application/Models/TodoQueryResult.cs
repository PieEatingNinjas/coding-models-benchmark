using TodoApi.Models;

namespace TodoApi.Application.Models;

public sealed record TodoQueryResult(IReadOnlyList<TodoItemDto> Items, int TotalCount);
