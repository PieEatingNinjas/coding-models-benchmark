using FluentAssertions;
using TodoApi.Models;
using TodoApi.Tests.Support;
using Xunit;

namespace TodoApi.Tests.Unit;

public class TodoMappingTests
{
    [Fact]
    public void ToDto_copies_public_fields()
    {
        var item = TodoItemFaker.Entity().Generate();

        var dto = item.ToDto();

        dto.Id.Should().Be(item.Id);
        dto.Name.Should().Be(item.Name);
        dto.IsComplete.Should().Be(item.IsComplete);
        dto.Priority.Should().Be(item.Priority);
        dto.DueDate.Should().Be(item.DueDate);
        dto.Tags.Should().BeEquivalentTo(item.Tags);
    }

    [Fact]
    public void Dto_never_exposes_secret()
    {
        // Compile-time + reflective guarantee that Secret cannot leak via the DTO.
        typeof(TodoItemDto).GetProperty("Secret").Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToDto_preserves_completion(bool complete)
    {
        var item = TodoItemFaker.Entity().Generate();
        item.IsComplete = complete;

        item.ToDto().IsComplete.Should().Be(complete);
    }

    [Fact]
    public void ToDto_default_priority_is_medium()
    {
        var item = new TodoItem();
        item.ToDto().Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public void ToDto_absent_tags_returns_empty_collection()
    {
        var item = new TodoItem { Tags = null! }; // simulating bad initialization if any
        item.ToDto().Tags.Should().NotBeNull().And.BeEmpty();
    }
}
