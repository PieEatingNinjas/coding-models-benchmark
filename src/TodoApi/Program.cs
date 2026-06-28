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

todos.MapGet("/", async (TodoDb db) =>
    await db.Todos.Select(t => t.ToDto()).ToListAsync());

todos.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).Select(t => t.ToDto()).ToListAsync());

todos.MapGet("/by-priority/{priority}", async (Priority priority, TodoDb db) =>
    await db.Todos.Where(t => t.Priority == priority).Select(t => t.ToDto()).ToListAsync());

todos.MapGet("/overdue", async (TodoDb db) =>
{
    var now = DateTimeOffset.UtcNow;
    return await db.Todos
        .Where(t => !t.IsComplete && t.DueDate.HasValue && t.DueDate < now)
        .Select(t => t.ToDto())
        .ToListAsync();
});

todos.MapGet("/{id:int}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) is { } todo
        ? Results.Ok(todo.ToDto())
        : Results.NotFound());

todos.MapPost("/", async (TodoItemDto dto, TodoDb db) =>
{
    if (dto.DueDate.HasValue && dto.DueDate.Value < DateTimeOffset.UtcNow)
    {
        return Results.Problem("Due date cannot be in the past.", statusCode: 400);
    }

    var todo = new TodoItem { Name = dto.Name, IsComplete = dto.IsComplete, Priority = dto.Priority, DueDate = dto.DueDate };
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo.ToDto());
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, TodoDb db) =>
{
    if (dto.DueDate.HasValue && dto.DueDate.Value < DateTimeOffset.UtcNow)
    {
        return Results.Problem("Due date cannot be in the past.", statusCode: 400);
    }

    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Name = dto.Name;
    todo.IsComplete = dto.IsComplete;
    todo.Priority = dto.Priority;
    todo.DueDate = dto.DueDate;
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
