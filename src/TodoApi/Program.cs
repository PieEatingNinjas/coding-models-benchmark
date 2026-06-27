using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todoitems");

todos.MapGet("/", async (string? tag, TodoDb db) =>
{
    var normalizedTag = TodoItem.NormalizeTag(tag);
    var items = await db.Todos.ToListAsync();
    var filtered = items
        .Where(todo => string.IsNullOrEmpty(normalizedTag) || todo.HasTag(normalizedTag))
        .Select(todo => todo.ToDto())
        .ToList();

    return Results.Ok(filtered);
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
public partial class Program { }
