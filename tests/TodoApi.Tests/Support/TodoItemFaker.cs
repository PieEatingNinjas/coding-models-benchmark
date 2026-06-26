using Bogus;
using TodoApi.Models;

namespace TodoApi.Tests.Support;

/// <summary>Deterministic-ish fake data builders (Bogus) for todos.</summary>
public static class TodoItemFaker
{
    public static Faker<TodoItemDto> Dto() => new Faker<TodoItemDto>()
        .RuleFor(t => t.Id, _ => 0)
        .RuleFor(t => t.Name, f => $"{f.Hacker.Verb()} {f.Hacker.Noun()}")
        .RuleFor(t => t.IsComplete, f => f.Random.Bool())
        .RuleFor(t => t.Priority, f => f.PickRandom<TodoPriority>())
        .RuleFor(t => t.DueDate, _ => null)
        .RuleFor(t => t.Tags, f => f.Random.ListItems(
            new[] { "work", "urgent", "home", "ops", "backend" },
            f.Random.Int(0, 3)));

    public static Faker<TodoItem> Entity() => new Faker<TodoItem>()
        .RuleFor(t => t.Id, _ => 0)
        .RuleFor(t => t.Name, f => $"{f.Hacker.Verb()} {f.Hacker.Noun()}")
        .RuleFor(t => t.IsComplete, f => f.Random.Bool())
        .RuleFor(t => t.Priority, f => f.PickRandom<TodoPriority>())
        .RuleFor(t => t.DueDate, _ => null)
        .RuleFor(t => t.Tags, f => f.Random.ListItems(
            new[] { "work", "urgent", "home", "ops", "backend" },
            f.Random.Int(0, 3)))
        .RuleFor(t => t.Secret, f => f.Internet.Password());
}
