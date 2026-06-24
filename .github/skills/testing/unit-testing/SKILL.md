---
name: unit-testing
description: How to write xUnit unit tests in this repo — AAA, FluentAssertions, Bogus, naming. Use when writing unit tests.
---

# Unit Testing (xUnit)

> **Related skills:** `testing/integration-testing`

## Stack
xUnit + FluentAssertions + Bogus. One test project per production project: `tests/<Project>.Tests`.

## Structure — Arrange-Act-Assert
```csharp
[Fact]
public void Withdraw_WithInsufficientFunds_Throws()
{
    // Arrange
    var account = new Account(balance: 50m);
    // Act
    var act = () => account.Withdraw(100m);
    // Assert
    act.Should().Throw<InvalidOperationException>();
}
```

## Naming
`Method_Scenario_ExpectedResult`. One logical behavior per test.

## Theory for data sets
```csharp
[Theory]
[InlineData(0, true)]
[InlineData(-1, false)]
public void IsValid_Cases(int input, bool expected) =>
    Validator.IsValid(input).Should().Be(expected);
```

## Fake data
- Use Bogus to generate realistic test data; keep generation in a shared faker (see `tests/.../Support`).
- Mock only real dependencies (external services, DB). Use real value objects.

## Rules
- No shared mutable state between tests; each test isolated.
- Meaningful asserts — never `Assert.True(true)`.
- Fast, deterministic tests (no `Thread.Sleep`, no real network/time dependency).
- Test behavior, not private implementation.
