using FluentAssertions;
using Moq;
using TodoApi.Application.DTOs;
using TodoApi.Application.Interfaces;
using TodoApi.Application.Services;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Enums;

namespace TodoApi.Application.Tests;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repoMock;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        _repoMock = new Mock<ITodoRepository>();
        _service = new TodoService(_repoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenItemExists()
    {
        // Arrange
        var entity = new TodoItem { Id = 1, Name = "Test", Secret = "hidden" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedDto()
    {
        // Arrange
        var dto = new TodoItemDto { Name = "New Todo", Priority = Priority.High };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TodoItem>()))
            .Callback<TodoItem>(t => t.Id = 1)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("New Todo");
        result.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenItemExists()
    {
        // Arrange
        var entity = new TodoItem { Id = 1, Name = "Old Name" };
        var dto = new TodoItemDto { Name = "New Name" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        result.Should().BeTrue();
        entity.Name.Should().Be("New Name"); // verifying the entity was mutated
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenItemDoesNotExist()
    {
        // Arrange
        var dto = new TodoItemDto { Name = "New Name" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        result.Should().BeFalse();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenItemExists()
    {
        // Arrange
        var entity = new TodoItem { Id = 1, Name = "Test" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
        _repoMock.Setup(r => r.DeleteAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenItemDoesNotExist()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeFalse();
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public void TodoItemDto_DoesNotContainSecretProperty()
    {
        // Act
        var secretProperty = typeof(TodoItemDto).GetProperty("Secret");

        // Assert
        secretProperty.Should().BeNull();
    }
}