using FluentAssertions;
using TodoApi.Application.Todos;
using TodoApi.Domain.Todos;
using TodoApi.Tests.Unit.Support;
using Xunit;

namespace TodoApi.Tests.Unit;

public class TodoServiceTests
{
    [Fact]
    public async Task GetTodos_returns_paged_items_and_total_count()
    {
        var repository = new MockTodoRepository
        {
            CountAsyncHandler = (_, _) => Task.FromResult(3),
            GetPageAsyncHandler = (_, skip, take, _) =>
            {
                skip.Should().Be(0);
                take.Should().Be(2);
                return Task.FromResult<IReadOnlyList<TodoItem>>(
                [
                    new TodoItem { Id = 1, Name = "a" },
                    new TodoItem { Id = 2, Name = "b" },
                ]);
            },
        };
        var service = new TodoService(repository);

        var result = await service.GetTodosAsync(null, page: 1, pageSize: 2, CancellationToken.None);

        result.ValidationErrors.Should().BeNull();
        result.TotalCount.Should().Be(3);
        result.Items.Select(t => t.Name).Should().Equal("a", "b");
    }

    [Fact]
    public async Task GetTodos_with_invalid_pagination_returns_validation_errors()
    {
        var service = new TodoService(new MockTodoRepository());

        var result = await service.GetTodosAsync(null, page: 0, pageSize: 0, CancellationToken.None);

        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors!.Keys.Should().Contain(["page", "pageSize"]);
    }

    [Fact]
    public async Task GetById_returns_null_when_not_found()
    {
        var service = new TodoService(new MockTodoRepository());

        var result = await service.GetByIdAsync(123, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Create_valid_todo_returns_dto_and_never_exposes_secret()
    {
        var repository = new MockTodoRepository
        {
            AddAsyncHandler = (todo, _) =>
            {
                todo.Id = 7;
                todo.Secret = "server-only";
                return Task.FromResult(todo);
            },
        };
        var service = new TodoService(repository);

        var result = await service.CreateAsync(new TodoItemDto
        {
            Name = "  New item  ",
            IsComplete = false,
            Tags = [" Work ", "work", "  "],
        }, CancellationToken.None);

        result.ValidationErrors.Should().BeNull();
        result.Todo.Should().NotBeNull();
        result.Todo!.Id.Should().Be(7);
        result.Todo.Tags.Should().Equal("work");
        typeof(TodoItemDto).GetProperty("Secret").Should().BeNull();
    }

    [Fact]
    public async Task Create_invalid_todo_returns_validation_errors()
    {
        var service = new TodoService(new MockTodoRepository());

        var result = await service.CreateAsync(new TodoItemDto
        {
            Name = "",
            DueDate = DateTimeOffset.UtcNow.AddDays(-1),
        }, CancellationToken.None);

        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors!.Keys.Should().Contain(["name", "dueDate"]);
    }

    [Fact]
    public async Task Update_existing_todo_succeeds()
    {
        var existing = new TodoItem { Id = 4, Name = "before", IsComplete = false };
        var saveCalls = 0;
        var repository = new MockTodoRepository
        {
            GetByIdAsyncHandler = (id, _) => Task.FromResult<TodoItem?>(id == 4 ? existing : null),
            SaveChangesAsyncHandler = _ =>
            {
                saveCalls++;
                return Task.CompletedTask;
            },
        };
        var service = new TodoService(repository);

        var result = await service.UpdateAsync(4, new TodoItemDto
        {
            Name = "after",
            IsComplete = true,
            Priority = TodoPriority.High,
            Tags = ["HOME", "home"],
        }, CancellationToken.None);

        result.NotFound.Should().BeFalse();
        result.ValidationErrors.Should().BeNull();
        saveCalls.Should().Be(1);
        existing.Name.Should().Be("after");
        existing.IsComplete.Should().BeTrue();
        existing.Priority.Should().Be(TodoPriority.High);
        existing.Tags.Should().Equal("home");
    }

    [Fact]
    public async Task Update_missing_todo_returns_not_found()
    {
        var service = new TodoService(new MockTodoRepository());

        var result = await service.UpdateAsync(404, new TodoItemDto { Name = "ignored" }, CancellationToken.None);

        result.NotFound.Should().BeTrue();
        result.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public async Task Delete_existing_todo_removes_and_saves()
    {
        var existing = new TodoItem { Id = 12, Name = "x" };
        var removed = false;
        var saveCalls = 0;
        var repository = new MockTodoRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<TodoItem?>(existing),
            RemoveHandler = todo => removed = ReferenceEquals(todo, existing),
            SaveChangesAsyncHandler = _ =>
            {
                saveCalls++;
                return Task.CompletedTask;
            },
        };
        var service = new TodoService(repository);

        var deleted = await service.DeleteAsync(12, CancellationToken.None);

        deleted.Should().BeTrue();
        removed.Should().BeTrue();
        saveCalls.Should().Be(1);
    }

    [Fact]
    public async Task Delete_missing_todo_returns_false()
    {
        var service = new TodoService(new MockTodoRepository());

        var deleted = await service.DeleteAsync(404, CancellationToken.None);

        deleted.Should().BeFalse();
    }
}
