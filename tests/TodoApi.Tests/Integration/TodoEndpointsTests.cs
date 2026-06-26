using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Tests.Support;
using Xunit;

namespace TodoApi.Tests.Integration;

public class TodoEndpointsTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>
{
    private readonly TodoApiFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_then_Get_returns_created_todo()
    {
        var dto = TodoItemFaker.Dto().Generate();

        var post = await _client.PostAsJsonAsync("/todoitems", dto);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();
        created!.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be(dto.Name);
        created.Priority.Should().Be(dto.Priority);

        var fetched = await _client.GetFromJsonAsync<TodoItemDto>($"/todoitems/{created.Id}");
        fetched!.Name.Should().Be(dto.Name);
    }

    [Fact]
    public async Task Get_unknown_returns_404()
    {
        var resp = await _client.GetAsync("/todoitems/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Complete_endpoint_returns_only_completed_items()
    {
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "done-1", IsComplete = true });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "done-2", IsComplete = true });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "open-1", IsComplete = false });

        var complete = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/complete");

        complete.Should().OnlyContain(t => t.IsComplete);
        complete.Should().Contain(t => t.Name == "done-1");
        complete.Should().NotContain(t => t.Name == "open-1");
    }

    [Fact]
    public async Task Put_updates_then_Delete_removes()
    {
        var post = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "original", IsComplete = false });
        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();

        var put = await _client.PutAsJsonAsync($"/todoitems/{created!.Id}",
            new TodoItemDto { Name = "updated", IsComplete = true, Priority = TodoPriority.High });
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterUpdate = await _client.GetFromJsonAsync<TodoItemDto>($"/todoitems/{created.Id}");
        afterUpdate!.Name.Should().Be("updated");
        afterUpdate.IsComplete.Should().BeTrue();
        afterUpdate.Priority.Should().Be(TodoPriority.High);

        var delete = await _client.DeleteAsync($"/todoitems/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDelete = await _client.GetAsync($"/todoitems/{created.Id}");
        afterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_without_priority_defaults_to_medium()
    {
        var post = await _client.PostAsJsonAsync("/todoitems", new
        {
            name = "no-priority",
            isComplete = false,
        });

        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();
        created!.Priority.Should().Be(TodoPriority.Medium);
    }

    [Fact]
    public async Task By_priority_returns_matching_items_only()
    {
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "high-item", IsComplete = false, Priority = TodoPriority.High });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "low-item", IsComplete = false, Priority = TodoPriority.Low });

        var high = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/by-priority/High");
        var low = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/by-priority/Low");

        high.Should().Contain(t => t.Name == "high-item");
        high.Should().NotContain(t => t.Name == "low-item");
        low.Should().Contain(t => t.Name == "low-item");
        low.Should().NotContain(t => t.Name == "high-item");
    }

    [Fact]
    public async Task By_priority_invalid_value_returns_400()
    {
        var response = await _client.GetAsync("/todoitems/by-priority/not-a-priority");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_with_past_due_date_returns_400_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = "past-post",
            IsComplete = false,
            DueDate = DateTimeOffset.UtcNow.AddDays(-7),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Invalid due date");
        problem.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_with_past_due_date_returns_400_problem_details()
    {
        var post = await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = "put-past-source",
            IsComplete = false,
            DueDate = DateTimeOffset.UtcNow.AddDays(3),
        });
        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();

        var response = await _client.PutAsJsonAsync($"/todoitems/{created!.Id}", new TodoItemDto
        {
            Name = "put-past-target",
            IsComplete = false,
            DueDate = DateTimeOffset.UtcNow.AddDays(-7),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Invalid due date");
        problem.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Overdue_excludes_items_without_due_date()
    {
        const string noDueDateName = "overdue-no-due-date";
        await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = noDueDateName,
            IsComplete = false,
            DueDate = null,
        });

        var overdue = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/overdue");

        overdue.Should().NotContain(t => t.Name == noDueDateName);
    }

    [Fact]
    public async Task Overdue_excludes_completed_items_with_expired_due_date()
    {
        const string completedExpiredName = "overdue-completed-expired-db";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
            db.Todos.Add(new TodoItem
            {
                Name = completedExpiredName,
                IsComplete = true,
                DueDate = DateTimeOffset.UtcNow.AddDays(-2),
            });
            await db.SaveChangesAsync();
        }

        var overdue = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/overdue");

        overdue.Should().NotContain(t => t.Name == completedExpiredName);
    }

    [Fact]
    public async Task Overdue_includes_incomplete_items_with_expired_due_date()
    {
        const string overdueName = "overdue-incomplete-expired-db";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
            db.Todos.Add(new TodoItem
            {
                Name = overdueName,
                IsComplete = false,
                DueDate = DateTimeOffset.UtcNow.AddDays(-2),
            });
            await db.SaveChangesAsync();
        }

        var overdue = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/overdue");

        overdue.Should().Contain(t => t.Name == overdueName);
    }
}
