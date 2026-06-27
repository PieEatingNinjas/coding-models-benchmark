using FluentAssertions;
using Moq;
using TodoApi.Application.Abstractions;
using TodoApi.Application.Exceptions;
using TodoApi.Application.Services;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests.Unit;

public class TodoItemServiceTests
{
    [Fact]
    public async Task GetByIdAsync_returns_dto_when_item_exists()
    {
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TodoItem { Id = 7, Name = "task", Secret = "hidden" });

        var service = new TodoItemService(repository.Object);

        var result = await service.GetByIdAsync(7);

        result.Should().NotBeNull();
        result!.Name.Should().Be("task");
        result.Id.Should().Be(7);
        typeof(TodoItemDto).GetProperty("Secret").Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_persists_item_and_returns_dto_without_secret()
    {
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(repo => repo.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem item, CancellationToken _) =>
            {
                item.Id = 42;
                return item;
            });

        var service = new TodoItemService(repository.Object);

        var created = await service.CreateAsync(new TodoItemDto { Name = "new task", IsComplete = false }, DateTimeOffset.UtcNow);

        created.Id.Should().Be(42);
        created.Name.Should().Be("new task");
        created.IsComplete.Should().BeFalse();
        repository.Verify(repo => repo.AddAsync(It.Is<TodoItem>(todo => todo.Name == "new task"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_item_is_missing()
    {
        var repository = new Mock<ITodoItemRepository>();
        repository.Setup(repo => repo.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TodoItem?)null);

        var service = new TodoItemService(repository.Object);

        var result = await service.UpdateAsync(99, new TodoItemDto { Name = "updated" }, DateTimeOffset.UtcNow);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_item_is_missing()
    {
        var repository = new Mock<ITodoItemRepository>();
        repository.Setup(repo => repo.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TodoItem?)null);

        var service = new TodoItemService(repository.Object);

        var deleted = await service.DeleteAsync(99);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_rejects_blank_name()
    {
        var repository = new Mock<ITodoItemRepository>();
        var service = new TodoItemService(repository.Object);

        var act = () => service.CreateAsync(new TodoItemDto { Name = "   " }, DateTimeOffset.UtcNow);

        await act.Should().ThrowAsync<TodoValidationException>();
        repository.Verify(repo => repo.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_rejects_past_due_date()
    {
        var repository = new Mock<ITodoItemRepository>();
        var service = new TodoItemService(repository.Object);

        var act = () => service.CreateAsync(new TodoItemDto { Name = "task", DueDate = DateTimeOffset.UtcNow.AddDays(-1) }, DateTimeOffset.UtcNow);

        await act.Should().ThrowAsync<TodoDomainException>();
        repository.Verify(repo => repo.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
