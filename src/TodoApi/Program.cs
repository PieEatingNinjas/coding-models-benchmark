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

todos.MapGet("/", async (HttpContext httpContext, string? tag, int? page, int? pageSize, TodoDb db) =>
{
    var normalizedTag = TodoItem.NormalizeTag(tag);
    var allItems = await db.Todos.ToListAsync();
    var filteredItems = allItems
        .Where(todo => string.IsNullOrEmpty(normalizedTag) || todo.HasTag(normalizedTag))
        .ToList();
    var totalCount = filteredItems.Count;

    var requestedPage = page ?? 1;
    var requestedPageSize = pageSize ?? 20;
    if (requestedPage < 1)
    {
        return CreatePaginationProblem("page", "page must be 1 or greater.");
    }

    if (requestedPageSize < 1)
    {
        return CreatePaginationProblem("pageSize", "pageSize must be 1 or greater.");
    }

    var effectivePageSize = Math.Min(requestedPageSize, 100);
    var skip = (requestedPage - 1) * effectivePageSize;
    var items = filteredItems
        .OrderBy(todo => todo.Id)
        .Skip(skip)
        .Take(effectivePageSize)
        .Select(todo => todo.ToDto())
        .ToList();

    httpContext.Response.Headers["X-Total-Count"] = totalCount.ToString();
    return Results.Ok(items);
});

todos.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).Select(t => t.ToDto()).ToListAsync());

todos.MapGet("/overdue", async (TodoDb db) =>
{
    var now = DateTimeOffset.UtcNow;
    var overdue = await db.Todos
        .Where(t => !t.IsComplete && t.DueDate.HasValue && t.DueDate.Value < now)
        .Select(t => t.ToDto())
        .ToListAsync();

    return Results.Ok(overdue);
});

todos.MapGet("/by-priority/{priority}", async (string priority, TodoDb db) =>
{
    if (!TodoPriorityExtensions.TryParsePriority(priority, out var parsedPriority))
    {
        return Results.BadRequest();
    }

    var filtered = await db.Todos
        .Where(t => t.Priority == parsedPriority)
        .Select(t => t.ToDto())
        .ToListAsync();

    return Results.Ok(filtered);
});

todos.MapGet("/{id:int}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) is { } todo
        ? Results.Ok(todo.ToDto())
        : Results.NotFound());

todos.MapPost("/", async (TodoItemDto dto, TodoDb db) =>
{
    var now = DateTimeOffset.UtcNow;
    if (dto.DueDate is { } dueDate && dueDate < now)
    {
        return Results.Problem(
            title: "Invalid due date",
            detail: "Due date cannot be in the past.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    if (TryGetNameValidationError(dto.Name, out var nameError))
    {
        return CreateValidationProblem("name", nameError);
    }

    var todo = new TodoItem
    {
        Name = dto.Name,
        IsComplete = dto.IsComplete,
        Priority = dto.Priority,
        DueDate = dto.DueDate,
    };
    todo.SetTags(dto.Tags);
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo.ToDto());
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, TodoDb db) =>
{
    var now = DateTimeOffset.UtcNow;
    if (dto.DueDate is { } dueDate && dueDate < now)
    {
        return Results.Problem(
            title: "Invalid due date",
            detail: "Due date cannot be in the past.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    if (TryGetNameValidationError(dto.Name, out var nameError))
    {
        return CreateValidationProblem("name", nameError);
    }

    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Name = dto.Name;
    todo.IsComplete = dto.IsComplete;
    todo.Priority = dto.Priority;
    todo.DueDate = dto.DueDate;
    todo.SetTags(dto.Tags);
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
public partial class Program
{
    private static bool TryGetNameValidationError(string? name, out string error)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Name is required.";
            return true;
        }

        if (name.Length > 200)
        {
            error = "Name must be 200 characters or fewer.";
            return true;
        }

        error = string.Empty;
        return false;
    }

    private static IResult CreateValidationProblem(string fieldName, string error) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [fieldName] = [error],
        });

    private static IResult CreatePaginationProblem(string fieldName, string error) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [fieldName] = [error],
        });
}
