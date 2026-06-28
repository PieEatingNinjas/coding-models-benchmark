using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
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
    public async Task Post_without_priority_applies_default_medium()
    {
        var dto = new TodoItemDto { Name = "no-priority", IsComplete = false };

        var post = await _client.PostAsJsonAsync("/todoitems", dto);
        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();

        created!.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public async Task Post_with_high_priority_then_filter_by_priority()
    {
        var highPriorityDto = new TodoItemDto { Name = "urgent", IsComplete = false, Priority = Priority.High };
        var mediumPriorityDto = new TodoItemDto { Name = "normal", IsComplete = false, Priority = Priority.Medium };

        await _client.PostAsJsonAsync("/todoitems", highPriorityDto);
        await _client.PostAsJsonAsync("/todoitems", mediumPriorityDto);

        var highPriorityTodos = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/by-priority/High");
        var mediumPriorityTodos = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems/by-priority/Medium");

        highPriorityTodos.Should().ContainSingle(t => t.Name == "urgent");
        highPriorityTodos.Should().NotContain(t => t.Name == "normal");
        mediumPriorityTodos.Should().ContainSingle(t => t.Name == "normal");
    }

    [Fact]
    public async Task By_priority_with_invalid_priority_returns_400()
    {
        var resp = await _client.GetAsync("/todoitems/by-priority/InvalidPriority");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_updates_priority()
    {
        var post = await _client.PostAsJsonAsync("/todoitems",
            new TodoItemDto { Name = "test", IsComplete = false, Priority = Priority.Low });
        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();

        var put = await _client.PutAsJsonAsync($"/todoitems/{created!.Id}",
            new TodoItemDto { Name = "test", IsComplete = false, Priority = Priority.High });
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updated = await _client.GetFromJsonAsync<TodoItemDto>($"/todoitems/{created.Id}");
        updated!.Priority.Should().Be(Priority.High);
    }
}
