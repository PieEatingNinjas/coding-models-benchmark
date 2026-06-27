using Microsoft.EntityFrameworkCore;
using TodoApi.Application.Abstractions;
using TodoApi.Application.Exceptions;
using TodoApi.Application.Services;
using TodoApi.Data;
using TodoApi.Infrastructure.Repositories;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<TodoItemService>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todoitems");

todos.MapGet("/", async (HttpContext httpContext, string? tag, int? page, int? pageSize, TodoItemService service) =>
{
    if (page is < 1)
    {
        return CreateValidationProblem(new Dictionary<string, string[]>
        {
            ["page"] = ["page must be 1 or greater."],
        });
    }

    if (pageSize is < 1)
    {
        return CreateValidationProblem(new Dictionary<string, string[]>
        {
            ["pageSize"] = ["pageSize must be 1 or greater."],
        });
    }

    var result = await service.GetItemsAsync(tag, page, pageSize);
    httpContext.Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
    return Results.Ok(result.Items);
});

todos.MapGet("/complete", async (TodoItemService service) => Results.Ok(await service.GetCompleteAsync()));

todos.MapGet("/overdue", async (TodoItemService service) => Results.Ok(await service.GetOverdueAsync(DateTimeOffset.UtcNow)));

todos.MapGet("/by-priority/{priority}", async (string priority, TodoItemService service) =>
{
    if (!TodoPriorityExtensions.TryParsePriority(priority, out var parsedPriority))
    {
        return Results.BadRequest();
    }

    return Results.Ok(await service.GetByPriorityAsync(parsedPriority));
});

todos.MapGet("/{id:int}", async (int id, TodoItemService service) =>
{
    var item = await service.GetByIdAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

todos.MapPost("/", async (TodoItemDto dto, TodoItemService service) =>
{
    try
    {
        var created = await service.CreateAsync(dto, DateTimeOffset.UtcNow);
        return Results.Created($"/todoitems/{created.Id}", created);
    }
    catch (TodoValidationException exception)
    {
        return CreateValidationProblem(exception.Errors);
    }
    catch (TodoDomainException exception)
    {
        return Results.Problem(
            title: exception.Title,
            detail: exception.Detail,
            statusCode: exception.StatusCode);
    }
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, TodoItemService service) =>
{
    try
    {
        var updated = await service.UpdateAsync(id, dto, DateTimeOffset.UtcNow);
        return updated is null ? Results.NotFound() : Results.NoContent();
    }
    catch (TodoValidationException exception)
    {
        return CreateValidationProblem(exception.Errors);
    }
    catch (TodoDomainException exception)
    {
        return Results.Problem(
            title: exception.Title,
            detail: exception.Detail,
            statusCode: exception.StatusCode);
    }
});

todos.MapDelete("/{id:int}", async (int id, TodoItemService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();

// Exposed so the integration tests can use WebApplicationFactory<Program>.
public partial class Program
{
    private static IResult CreateValidationProblem(IReadOnlyDictionary<string, string[]> errors) =>
        Results.ValidationProblem(errors.ToDictionary(item => item.Key, item => item.Value));
}
