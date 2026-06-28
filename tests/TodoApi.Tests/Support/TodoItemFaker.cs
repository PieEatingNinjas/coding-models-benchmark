using Bogus;
using TodoApi.Application.DTOs;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Enums;

namespace TodoApi.Tests.Support;

/// <summary>Deterministic-ish fake data builders (Bogus) for todos.</summary>
public static class TodoItemFaker
{
    public static Faker<TodoItemDto> Dto() => new Faker<TodoItemDto>()
        .RuleFor(t => t.Id, _ => 0)
        .RuleFor(t => t.Name, f => $"{f.Hacker.Verb()} {f.Hacker.Noun()}")
        .RuleFor(t => t.IsComplete, f => f.Random.Bool())
        .RuleFor(t => t.Priority, f => f.PickRandom<Priority>())
        .RuleFor(t => t.Tags, f => f.Random.WordsArray(1, 3).ToList());

    public static Faker<TodoItem> Entity() => new Faker<TodoItem>()
        .RuleFor(t => t.Id, _ => 0)
        .RuleFor(t => t.Name, f => $"{f.Hacker.Verb()} {f.Hacker.Noun()}")
        .RuleFor(t => t.IsComplete, f => f.Random.Bool())
        .RuleFor(t => t.Priority, f => f.PickRandom<Priority>())
        .RuleFor(t => t.Secret, f => f.Internet.Password())
        .RuleFor(t => t.Tags, f => f.Random.WordsArray(1, 3).ToList());
}
