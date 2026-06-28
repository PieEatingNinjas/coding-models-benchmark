using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task Post_without_priority_defaults_to_medium()
    {
        var dto = new TodoItemDto { Name = "default-priority", IsComplete = false };

        var post = await _client.PostAsJsonAsync("/todoitems", dto);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();
        created!.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public async Task Filter_by_priority_returns_matching_items()
    {
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "high-prio", Priority = Priority.High });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "low-prio", Priority = Priority.Low });

        var highItems = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/by-priority/High");
        highItems.Should().Contain(t => t.Name == "high-prio");
        highItems.Should().NotContain(t => t.Name == "low-prio");

        var lowItems = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/by-priority/Low");
        lowItems.Should().Contain(t => t.Name == "low-prio");
        lowItems.Should().NotContain(t => t.Name == "high-prio");
    }

    [Fact]
    public async Task Get_by_invalid_priority_returns_400()
    {
        var resp = await _client.GetAsync("/todoitems/by-priority/Urgent");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_with_past_due_date_returns_400_ProblemDetails()
    {
        var dto = new TodoItemDto
        {
            Name = "past-due",
            DueDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var post = await _client.PostAsJsonAsync("/todoitems", dto);
        post.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await post.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().NotBeNull();
    }

    [Fact]
    public async Task Put_with_past_due_date_returns_400()
    {
        var post = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "valid-due-date", DueDate = DateTimeOffset.UtcNow.AddDays(1) });
        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();

        var put = await _client.PutAsJsonAsync($"/todoitems/{created!.Id}",
            new TodoItemDto { Name = "invalid-due-date", DueDate = DateTimeOffset.UtcNow.AddDays(-1) });

        put.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Overdue_filter_works_correctly()
    {
        var past = DateTimeOffset.UtcNow.AddDays(-1);
        var future = DateTimeOffset.UtcNow.AddDays(1);

        // We bypass the API validation to insert past due dates
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoApi.Data.TodoDb>();

        db.Todos.AddRange(
            new TodoItem { Name = "no-due-date" },
            new TodoItem { Name = "completed-past", IsComplete = true, DueDate = past },
            new TodoItem { Name = "incomplete-future", DueDate = future },
            new TodoItem { Name = "incomplete-past", DueDate = past }
        );
        await db.SaveChangesAsync();

        var overdue = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/overdue");

        overdue.Should().NotBeNull();
        overdue!.Should().Contain(t => t.Name == "incomplete-past");
        overdue.Should().NotContain(t => t.Name == "no-due-date");
        overdue.Should().NotContain(t => t.Name == "completed-past");
        overdue.Should().NotContain(t => t.Name == "incomplete-future");
    }

    [Fact]
    public async Task Filter_by_tag_works_case_insensitively_and_excludes_non_matches()
    {
        // Integration: POST with tags ["work","urgent"] -> findable via ?tag=work and ?tag=WORK.
        // Integration: a todo without the requested tag does not appear in the filtered result.

        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "tagged-1", Tags = ["work", "urgent"] });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "tagged-2", Tags = ["home"] });

        var match1 = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems?tag=work");
        match1.Should().Contain(t => t.Name == "tagged-1");
        match1.Should().NotContain(t => t.Name == "tagged-2");

        var match2 = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems?tag=WORK");
        match2.Should().Contain(t => t.Name == "tagged-1");
        match2.Should().NotContain(t => t.Name == "tagged-2");
    }

    [Fact]
    public async Task Get_without_tag_parameter_returns_all_todos()
    {
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "all-1", Tags = ["t1"] });
        await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "all-2", Tags = ["t2"] });

        var all = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems");

        all.Should().NotBeNull();
        all!.Should().Contain(t => t.Name == "all-1");
        all.Should().Contain(t => t.Name == "all-2");
    }
}
