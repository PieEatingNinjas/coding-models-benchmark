using Microsoft.AspNetCore.Mvc;
using TodoApi.Application;
using TodoApi.Application.DTOs;
using TodoApi.Application.Interfaces;
using TodoApi.Domain.Enums;
using TodoApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todoitems");

todos.MapGet("/", async (ITodoService service, HttpResponse response, string? tag = null, int page = 1, int pageSize = 20) =>
{
    var (items, totalCount) = await service.GetPaginatedAsync(tag, page, pageSize);
    response.Headers.Append("X-Total-Count", totalCount.ToString());
    return items.ToList();
});

todos.MapGet("/complete", async (ITodoService service) =>
    await service.GetCompleteAsync());

todos.MapGet("/by-priority/{priority}", async (Priority priority, ITodoService service) =>
    await service.GetByPriorityAsync(priority));

todos.MapGet("/overdue", async (ITodoService service) =>
    await service.GetOverdueAsync());

todos.MapGet("/{id:int}", async (int id, ITodoService service) =>
    await service.GetByIdAsync(id) is { } dto
        ? Results.Ok(dto)
        : Results.NotFound());

todos.MapPost("/", async (TodoItemDto dto, ITodoService service) =>
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

    var created = await service.CreateAsync(dto);
    return Results.Created($"/todoitems/{created.Id}", created);
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, ITodoService service) =>
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

    var success = await service.UpdateAsync(id, dto);
    return success ? Results.NoContent() : Results.NotFound();
});

todos.MapDelete("/{id:int}", async (int id, ITodoService service) =>
{
    var success = await service.DeleteAsync(id);
    return success ? Results.NoContent() : Results.NotFound();
});

app.Run();

// Exposed so the integration tests can use WebApplicationFactory<Program>.
public partial class Program { }

