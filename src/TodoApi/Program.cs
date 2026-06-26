using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todoitems");

const int DefaultPage = 1;
const int DefaultPageSize = 20;
const int MaxPageSize = 100;
const string TotalCountHeaderName = "X-Total-Count";

static bool DueDateIsInPast(DateTimeOffset? dueDate) =>
    dueDate is { } value && value < DateTimeOffset.UtcNow;

static string NormalizeTag(string tag) => tag.Trim().ToLowerInvariant();

static List<string> NormalizeTags(IEnumerable<string>? tags)
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

static Dictionary<string, string[]> ValidateTodoInput(TodoItemDto dto)
{
    Dictionary<string, string[]> errors = [];

    if (string.IsNullOrWhiteSpace(dto.Name))
    {
        errors["name"] = ["Name is required."];
    }

    if (dto.Name.Length > 200)
    {
        errors["name"] = ["Name must be 200 characters or fewer."];
    }

    if (DueDateIsInPast(dto.DueDate))
    {
        errors["dueDate"] = ["DueDate cannot be in the past."];
    }

    return errors;
}

static Dictionary<string, string[]> ValidatePagination(int page, int pageSize)
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

todos.MapGet("/", async (string? tag, int? page, int? pageSize, TodoDb db, HttpContext httpContext) =>
{
    var resolvedPage = page ?? DefaultPage;
    var resolvedPageSize = pageSize ?? DefaultPageSize;

    var pagingErrors = ValidatePagination(resolvedPage, resolvedPageSize);
    if (pagingErrors.Count > 0)
    {
        return Results.ValidationProblem(pagingErrors);
    }

    if (resolvedPageSize > MaxPageSize)
    {
        resolvedPageSize = MaxPageSize;
    }

    var query = db.Todos.AsQueryable();
    if (!string.IsNullOrWhiteSpace(tag))
    {
        var normalizedTag = NormalizeTag(tag);
        query = query.Where(todo => todo.Tags.Contains(normalizedTag));
    }

    var totalCount = await query.CountAsync();
    var skip = (resolvedPage - 1) * resolvedPageSize;
    var items = await query
        .OrderBy(t => t.Id)
        .Skip(skip)
        .Take(resolvedPageSize)
        .Select(t => t.ToDto())
        .ToListAsync();

    httpContext.Response.Headers[TotalCountHeaderName] = totalCount.ToString();

    return Results.Ok(items);
});

todos.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).Select(t => t.ToDto()).ToListAsync());

todos.MapGet("/overdue", async (TodoDb db) =>
{
    var now = DateTimeOffset.UtcNow;
    return await db.Todos
        .Where(t => !t.IsComplete && t.DueDate.HasValue && t.DueDate.Value < now)
        .Select(t => t.ToDto())
        .ToListAsync();
});

todos.MapGet("/by-priority/{priority}", async (string priority, TodoDb db) =>
{
    if (!Enum.TryParse<TodoPriority>(priority, ignoreCase: true, out var parsedPriority) ||
        !Enum.IsDefined(parsedPriority))
    {
        return Results.BadRequest($"Invalid priority '{priority}'. Expected one of: Low, Medium, High.");
    }

    return Results.Ok(await db.Todos
        .Where(t => t.Priority == parsedPriority)
        .Select(t => t.ToDto())
        .ToListAsync());
});

todos.MapGet("/{id:int}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) is { } todo
        ? Results.Ok(todo.ToDto())
        : Results.NotFound());

todos.MapPost("/", async (TodoItemDto dto, TodoDb db) =>
{
    var validationErrors = ValidateTodoInput(dto);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var todo = new TodoItem
    {
        Name = dto.Name,
        IsComplete = dto.IsComplete,
        Priority = dto.Priority,
        DueDate = dto.DueDate,
        Tags = NormalizeTags(dto.Tags),
    };
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo.ToDto());
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    var validationErrors = ValidateTodoInput(dto);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    todo.Name = dto.Name;
    todo.IsComplete = dto.IsComplete;
    todo.Priority = dto.Priority;
    todo.DueDate = dto.DueDate;
    todo.Tags = NormalizeTags(dto.Tags);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

todos.MapDelete("/{id:int}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is { } todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();

// Exposed so the integration tests can use WebApplicationFactory<Program>.
public partial class Program { }
