using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todoitems");

todos.MapGet("/", async (TodoDb db, HttpResponse response, string? tag = null, int page = 1, int pageSize = 20) =>
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var todos = await db.Todos.ToListAsync();
    var query = todos.AsEnumerable();
    if (!string.IsNullOrWhiteSpace(tag))
    {
        query = query.Where(t => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }

    var totalCount = query.Count();
    response.Headers.Append("X-Total-Count", totalCount.ToString());

    return query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => t.ToDto())
        .ToList();
});

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
    if (string.IsNullOrWhiteSpace(dto.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "Name", ["Name is required."] } });
    }
    if (dto.Name.Length > 200)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "Name", ["Name cannot exceed 200 characters."] } });
    }
    if (dto.DueDate.HasValue && dto.DueDate.Value < DateTimeOffset.UtcNow)
    {
        return Results.Problem("Due date cannot be in the past.", statusCode: 400);
    }

    var tags = dto.Tags?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
    var todo = new TodoItem { Name = dto.Name, IsComplete = dto.IsComplete, Priority = dto.Priority, DueDate = dto.DueDate, Tags = tags };
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo.ToDto());
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, TodoDb db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "Name", ["Name is required."] } });
    }
    if (dto.Name.Length > 200)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "Name", ["Name cannot exceed 200 characters."] } });
    }
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
    todo.Tags = dto.Tags?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
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
