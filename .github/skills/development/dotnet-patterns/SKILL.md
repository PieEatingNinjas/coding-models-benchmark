---
name: dotnet-patterns
description: Standard .NET/C# patterns for this repo — DI, immutable models, async I/O, options, error handling. Use before writing services or models.
---

# .NET Patterns

> **Related skills:** `development/implementation-workflow`, `development/code-checklist`

## Pattern 1 — Dependency Injection
Register services in `Program.cs`/`Startup` via the built-in container. Inject via constructor. No `static` singletons, no service locator.

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
```

## Pattern 2 — Immutable models
Use `record` for value objects/DTOs; validate invariants in the constructor.

```csharp
public sealed record Money(decimal Amount, string Currency)
{
    public Money { if (Amount < 0) throw new ArgumentOutOfRangeException(nameof(Amount)); }
}
```

## Pattern 3 — Async I/O end-to-end
All I/O `async`; pass `CancellationToken` through. Never `.Result`/`.Wait()`/`.GetAwaiter().GetResult()`.

```csharp
public async Task<Order?> GetAsync(int id, CancellationToken ct) =>
    await _db.Orders.FindAsync([id], ct);
```

## Pattern 4 — Options
Configuration via strongly-typed options (`IOptions<T>`), not loose `IConfiguration` reads deep in the code.

## Pattern 5 — Error handling
- Throw specific exceptions; catch only what you can handle.
- Validate input at the edge (model validation / guard clauses).
- Money = `decimal`. Timestamps = `DateTimeOffset`.

## Pattern 6 — Results over exceptions (optional)
For expected error paths: a `Result<T>` type instead of exceptions for control flow.

## Anti-patterns (avoid)
- `async void` (except event handlers).
- Swallowed exceptions (`catch {}`).
- God classes / static mutable state.
- Over-abstraction: no interface with a single implementation "just in case".
