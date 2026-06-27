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

        var fetched = await _client.GetFromJsonAsync<TodoItemDto>($"/todoitems/{created.Id}");
        fetched!.Name.Should().Be(dto.Name);
    }

    [Fact]
    public async Task Post_without_priority_defaults_to_medium()
    {
        var response = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "default-priority", IsComplete = false });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<TodoItemDto>();
        created!.Priority.Should().Be(TodoPriority.Medium);
    }

    [Fact]
    public async Task Post_without_name_returns_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { IsComplete = false });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
        problem.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public async Task Post_with_name_longer_than_200_characters_returns_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = new string('a', 201), IsComplete = false });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
    }

    [Fact]
    public async Task Post_with_past_due_date_returns_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "past-due", IsComplete = false, DueDate = DateTimeOffset.UtcNow.AddDays(-1) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Invalid due date");
        problem.Status.Should().Be(400);
    }

    [Fact]
    public async Task Put_with_past_due_date_returns_problem_details()
    {
        var create = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "put-past-due", IsComplete = false });
        var created = await create.Content.ReadFromJsonAsync<TodoItemDto>();

        var response = await _client.PutAsJsonAsync($"/todoitems/{created!.Id}",
            new TodoItemDto { Name = "put-past-due", IsComplete = false, DueDate = DateTimeOffset.UtcNow.AddDays(-1) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Invalid due date");
        problem.Status.Should().Be(400);
    }

    [Fact]
    public async Task Overdue_endpoint_returns_only_incomplete_expired_todos()
    {
        var prefix = Guid.NewGuid().ToString("N");
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();

        db.Todos.Add(new TodoItem { Name = $"{prefix}-no-due-date", IsComplete = false });
        db.Todos.Add(new TodoItem { Name = $"{prefix}-completed-expired", IsComplete = true, DueDate = pastDate });
        db.Todos.Add(new TodoItem { Name = $"{prefix}-incomplete-expired", IsComplete = false, DueDate = pastDate });
        await db.SaveChangesAsync();

        var overdue = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/overdue");

        overdue.Should().ContainSingle(item => item.Name == $"{prefix}-incomplete-expired");
        overdue.Should().NotContain(item => item.Name == $"{prefix}-no-due-date");
        overdue.Should().NotContain(item => item.Name == $"{prefix}-completed-expired");
    }

    [Fact]
    public async Task By_priority_endpoint_filters_and_rejects_invalid_values()
    {
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "high-item", IsComplete = false, Priority = TodoPriority.High });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "low-item", IsComplete = false, Priority = TodoPriority.Low });

        var highItems = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/by-priority/High");
        highItems.Should().ContainSingle(item => item.Name == "high-item");
        highItems.Should().NotContain(item => item.Name == "low-item");

        var invalid = await _client.GetAsync("/todoitems/by-priority/urgent");
        invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_with_tag_filters_case_insensitively_and_returns_all_without_filter()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        db.Todos.RemoveRange(db.Todos);
        await db.SaveChangesAsync();

        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "tagged-work", IsComplete = false, Tags = ["work", "urgent"] });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "tagged-urgent", IsComplete = false, Tags = ["urgent"] });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "untagged", IsComplete = false });

        var filtered = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems?tag=WORK");
        filtered.Should().ContainSingle(item => item.Name == "tagged-work");
        filtered.Should().NotContain(item => item.Name == "tagged-urgent");
        filtered.Should().NotContain(item => item.Name == "untagged");

        var all = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems");
        all.Should().HaveCount(3);
    }

    [Fact]
    public async Task Get_returns_paginated_items_with_total_count_header_and_defaults()
    {
        await ResetDatabaseAsync();

        for (var i = 0; i < 25; i++)
        {
            await _client.PostAsJsonAsync("/todoitems", new TodoItemDto { Name = $"todo-{i}", IsComplete = false });
        }

        var firstPage = await _client.GetAsync("/todoitems?page=1&pageSize=20");
        firstPage.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Headers.GetValues("X-Total-Count").Should().ContainSingle().Which.Should().Be("25");

        var firstPageItems = await firstPage.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        firstPageItems.Should().HaveCount(20);

        var secondPage = await _client.GetAsync("/todoitems?page=2&pageSize=20");
        secondPage.StatusCode.Should().Be(HttpStatusCode.OK);
        secondPage.Headers.GetValues("X-Total-Count").Should().ContainSingle().Which.Should().Be("25");

        var secondPageItems = await secondPage.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        secondPageItems.Should().HaveCount(5);

        var defaultPage = await _client.GetAsync("/todoitems");
        defaultPage.StatusCode.Should().Be(HttpStatusCode.OK);
        defaultPage.Headers.GetValues("X-Total-Count").Should().ContainSingle().Which.Should().Be("25");

        var defaultItems = await defaultPage.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        defaultItems.Should().HaveCount(20);
    }

    [Fact]
    public async Task Get_clamps_page_size_to_upper_bound_and_rejects_invalid_page_values()
    {
        await ResetDatabaseAsync();

        for (var i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/todoitems", new TodoItemDto { Name = $"page-size-{i}", IsComplete = false });
        }

        var clamped = await _client.GetAsync("/todoitems?page=1&pageSize=500");
        clamped.StatusCode.Should().Be(HttpStatusCode.OK);
        var clampedItems = await clamped.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        clampedItems.Should().HaveCount(5);

        var invalidPage = await _client.GetAsync("/todoitems?page=0");
        invalidPage.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await invalidPage.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
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
            new TodoItemDto { Name = "updated", IsComplete = true });
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterUpdate = await _client.GetFromJsonAsync<TodoItemDto>($"/todoitems/{created.Id}");
        afterUpdate!.Name.Should().Be("updated");
        afterUpdate.IsComplete.Should().BeTrue();

        var delete = await _client.DeleteAsync($"/todoitems/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDelete = await _client.GetAsync($"/todoitems/{created.Id}");
        afterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task ResetDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        db.Todos.RemoveRange(db.Todos);
        await db.SaveChangesAsync();
    }
}
