using TodoApi.Application.Todos;
using TodoApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTodoInfrastructure("TodoList");
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todoitems");

const string TotalCountHeaderName = "X-Total-Count";

todos.MapGet("/", async (
    string? tag,
    int? page,
    int? pageSize,
    ITodoService service,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var result = await service.GetTodosAsync(tag, page, pageSize, cancellationToken);
    if (result.ValidationErrors is not null)
    {
        return Results.ValidationProblem(result.ValidationErrors);
    }

    httpContext.Response.Headers[TotalCountHeaderName] = result.TotalCount.ToString();
    return Results.Ok(result.Items);
});

todos.MapGet("/complete", async (ITodoService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetCompleteAsync(cancellationToken)));

todos.MapGet("/overdue", async (ITodoService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetOverdueAsync(cancellationToken)));

todos.MapGet("/by-priority/{priority}", async (string priority, ITodoService service, CancellationToken cancellationToken) =>
{
    var result = await service.GetByPriorityAsync(priority, cancellationToken);
    if (result.ErrorMessage is not null)
    {
        return Results.BadRequest(result.ErrorMessage);
    }

    return Results.Ok(result.Items);
});

todos.MapGet("/{id:int}", async (int id, ITodoService service, CancellationToken cancellationToken) =>
    await service.GetByIdAsync(id, cancellationToken) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

todos.MapPost("/", async (TodoItemDto dto, ITodoService service, CancellationToken cancellationToken) =>
{
    var result = await service.CreateAsync(dto, cancellationToken);
    if (result.ValidationErrors is not null)
    {
        return Results.ValidationProblem(result.ValidationErrors);
    }

    var created = result.Todo!;
    return Results.Created($"/todoitems/{created.Id}", created);
});

todos.MapPut("/{id:int}", async (int id, TodoItemDto dto, ITodoService service, CancellationToken cancellationToken) =>
{
    var result = await service.UpdateAsync(id, dto, cancellationToken);
    if (result.NotFound)
    {
        return Results.NotFound();
    }

    if (result.ValidationErrors is not null)
    {
        return Results.ValidationProblem(result.ValidationErrors);
    }

    return Results.NoContent();
});

todos.MapDelete("/{id:int}", async (int id, ITodoService service, CancellationToken cancellationToken) =>
    await service.DeleteAsync(id, cancellationToken) ? Results.NoContent() : Results.NotFound());

app.Run();

// Exposed so the integration tests can use WebApplicationFactory<Program>.
public partial class Program { }
