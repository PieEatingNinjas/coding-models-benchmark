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

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        db.Todos.RemoveRange(db.Todos);
        await db.SaveChangesAsync();
    }

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
        problem!.Status.Should().Be((int)HttpStatusCode.BadRequest);
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
        problem!.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_without_name_returns_400_validation_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/todoitems", new
        {
            isComplete = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("name");
    }

    [Fact]
    public async Task Post_with_name_longer_than_200_returns_400_validation_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = new string('x', 201),
            IsComplete = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("name");
    }

    [Fact]
    public async Task Get_with_pagination_returns_expected_pages_and_total_count_header()
    {
        await ResetDatabaseAsync();
        for (var i = 0; i < 25; i++)
        {
            await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
            {
                Name = $"page-item-{i}",
                IsComplete = false,
            });
        }

        var page1Response = await _client.GetAsync("/todoitems?page=1&pageSize=20");
        page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        page1Response.Headers.TryGetValues("X-Total-Count", out var page1Totals).Should().BeTrue();
        page1Totals!.Single().Should().Be("25");
        var page1 = await page1Response.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        page1.Should().NotBeNull();
        page1!.Should().HaveCount(20);

        var page2Response = await _client.GetAsync("/todoitems?page=2&pageSize=20");
        page2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        page2Response.Headers.TryGetValues("X-Total-Count", out var page2Totals).Should().BeTrue();
        page2Totals!.Single().Should().Be("25");
        var page2 = await page2Response.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        page2.Should().NotBeNull();
        page2!.Should().HaveCount(5);
    }

    [Fact]
    public async Task Get_without_pagination_parameters_applies_defaults()
    {
        await ResetDatabaseAsync();
        for (var i = 0; i < 25; i++)
        {
            await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
            {
                Name = $"default-page-item-{i}",
                IsComplete = false,
            });
        }

        var response = await _client.GetAsync("/todoitems");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Total-Count", out var totals).Should().BeTrue();
        totals!.Single().Should().Be("25");

        var items = await response.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        items.Should().NotBeNull();
        items!.Should().HaveCount(20);
    }

    [Fact]
    public async Task Get_with_page_size_above_limit_is_clamped_to_100()
    {
        await ResetDatabaseAsync();
        for (var i = 0; i < 120; i++)
        {
            await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
            {
                Name = $"clamped-page-item-{i}",
                IsComplete = false,
            });
        }

        var response = await _client.GetAsync("/todoitems?page=1&pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Total-Count", out var totals).Should().BeTrue();
        totals!.Single().Should().Be("120");

        var items = await response.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        items.Should().NotBeNull();
        items!.Should().HaveCount(100);
    }

    [Fact]
    public async Task Get_with_invalid_page_returns_400_validation_problem_details()
    {
        var response = await _client.GetAsync("/todoitems?page=0&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("page");
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

    [Fact]
    public async Task Get_with_tag_filter_is_case_insensitive()
    {
        await ResetDatabaseAsync();
        await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = "tagged-work-item",
            IsComplete = false,
            Tags = ["work", "urgent"],
        });
        await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = "tagged-home-item",
            IsComplete = false,
            Tags = ["home"],
        });

        var lowerCase = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems?tag=work");
        var upperCase = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems?tag=WORK");

        lowerCase.Should().Contain(t => t.Name == "tagged-work-item");
        lowerCase.Should().NotContain(t => t.Name == "tagged-home-item");
        upperCase.Should().Contain(t => t.Name == "tagged-work-item");
        upperCase.Should().NotContain(t => t.Name == "tagged-home-item");
    }

    [Fact]
    public async Task Get_with_no_tag_parameter_returns_all_items()
    {
        await ResetDatabaseAsync();
        await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = "all-items-a",
            IsComplete = false,
            Tags = ["work"],
        });
        await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = "all-items-b",
            IsComplete = false,
            Tags = [],
        });

        var allItems = await _client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems");

        allItems.Should().Contain(t => t.Name == "all-items-a");
        allItems.Should().Contain(t => t.Name == "all-items-b");
    }

    [Fact]
    public async Task Put_overwrites_tags()
    {
        var post = await _client.PostAsJsonAsync("/todoitems", new TodoItemDto
        {
            Name = "put-tags-source",
            IsComplete = false,
            Tags = ["work", "urgent"],
        });
        var created = await post.Content.ReadFromJsonAsync<TodoItemDto>();

        var put = await _client.PutAsJsonAsync($"/todoitems/{created!.Id}", new TodoItemDto
        {
            Name = "put-tags-source",
            IsComplete = false,
            Tags = ["home"],
        });
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await _client.GetFromJsonAsync<TodoItemDto>($"/todoitems/{created.Id}");
        fetched.Should().NotBeNull();
        fetched!.Tags.Should().Equal("home");
    }
}
