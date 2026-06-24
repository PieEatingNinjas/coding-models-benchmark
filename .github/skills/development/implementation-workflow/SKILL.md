---
name: implementation-workflow
description: Step-by-step implementation order for .NET code from specs. Use at the start of a new implementation or when planning the order.
---

# .NET Implementation Workflow

> **Related skills:** `development/dotnet-patterns`, `development/code-checklist`, `build/build-validation`

## Prerequisites

- Specs exist in `spec/` (start at `INDEX.md`).
- .NET SDK + solution (`src/<Solution>.sln`) present.

## Phase 0 — Context
1. Read `spec/INDEX.md` fully.
2. Filter by category/tags for the part you are implementing.
3. Read all relevant specs before writing code.

## Implementation order

Implement inside-out — dependencies first:

1. **Domain models & value objects** — records/classes, invariants in the constructor. (`dotnet-patterns`)
2. **Contracts/interfaces** — abstractions that services rely on.
3. **Services / business logic** — pure logic, DI-injectable.
4. **Persistence / I/O** — EF Core, repositories, external calls (`async`).
5. **Endpoints / entry point** — controllers/minimal API or Program.cs.

> **Build after EVERY class** (`dotnet build`). Do not continue on a red build. Just implement it — no premature optimization.

## Validation

```bash
dotnet build -warnaserror      # zero warnings
dotnet format --verify-no-changes
dotnet test                    # if tests present
dotnet run --project src/<App> # smoke test against the spec
```

## Rules

- `decimal` for money; `async`/`await` for I/O; DI instead of statics.
- No unsolicited refactors. One public type per file.
- Verify uncertain APIs via Microsoft Learn MCP.
