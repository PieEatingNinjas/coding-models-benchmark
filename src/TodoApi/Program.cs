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
    var todo = new TodoItem { Name = dto.Name, IsComplete = dto.IsComplete, Priority = dto.Priority };
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo.ToDto());
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Name = dto.Name;
    todo.IsComplete = dto.IsComplete;
    todo.Priority = dto.Priority;
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
