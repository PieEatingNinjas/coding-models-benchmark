# Architecture Review — F5 Clean Architecture (end state)

**Scope reviewed:** `git diff baseline...HEAD` ending at `bd18805` (F5), against `.github/spec/features/F5-clean-architecture.md` and the repo ground rules.
**Build/test floor:** `dotnet build -warnaserror` → 0 warnings / 0 errors across all 5 projects. `dotnet test` → 30/30 passing.

> Supersedes an earlier "not-applicable — playthrough has not reached F5" review that was written when `HEAD` was detached at F1 and the layer folders held only stale `bin/obj` output. `HEAD` is now `bd18805` (F5) and the layer split is real and tracked.

## Verdict

**Solid.** A near-textbook Clean Architecture split that preserves behavior. Dependency direction is correct, there is no persistence leak into the API, and pagination is pushed to the query rather than done in memory. The only findings are low-severity convention/consistency smells — no architectural problems.

## What's right

- **Dependency direction is clean and inward-only.** Domain references nothing (`TodoApi.Domain.csproj` has no `ProjectReference`); Application → Domain; Infrastructure → Application + Domain; API → Application + Infrastructure (composition root only). No outward or sideways leaks.
- **No EF/persistence leak in the API.** `Program.cs` contains zero `DbContext`/EF references — it wires persistence through `AddTodoInfrastructure("TodoList")` (Infrastructure DI extension) and depends only on the `ITodoService`/`ITodoRepository` abstractions. Exactly the pattern the checklist looks for.
- **Pagination/filtering is queryable, not in-memory.** `TodoRepository.GetPageAsync` applies `OrderBy(...).Skip(skip).Take(take).ToListAsync(...)` and `CountAsync` runs `CountAsync` on the filtered `IQueryable` — the easiest thing to get wrong, done right. `ITodoRepository` is paging-aware (`CountAsync` + `GetPageAsync(skip, take)`), so the endpoint never over-fetches.
- **Thin endpoints + DTO/Secret rule held.** Endpoints only validate-translate-delegate; all business logic and validation live in `TodoService`, with a shared `ValidateTodoInput` used by both POST and PUT (no duplicated validation). `TodoItemDto` sits in Application and has no `Secret` property; the rule is now asserted at the service level (`TodoServiceTests.Create_valid_todo_returns_dto_and_never_exposes_secret`).

## Findings (most → least severe)

1. **smell · `src/TodoApi.Domain/TodoApi.Domain.csproj:3-7`, `src/TodoApi.Application/TodoApi.Application.csproj:7-11`, `src/TodoApi.Infrastructure/TodoApi.Infrastructure.csproj:13-17`** — each new project re-declares `TargetFramework`, `Nullable`, and `ImplicitUsings`, which the root `Directory.Build.props` already provides. Ground rules say "shared build settings live in `Directory.Build.props`; don't duplicate or relax them per project." *Why it matters:* redundant settings drift over time and invite per-project divergence. (They do **not** redeclare `TreatWarningsAsErrors`/`LangVersion`, so warnings-as-errors is **not** relaxed — the `-warnaserror` build is clean across all three; this is pure duplication, not a relaxation.) *Fix:* delete those `PropertyGroup`s and inherit from `Directory.Build.props`, like `src/TodoApi/TodoApi.csproj` already correctly does.

2. **nitpick · `src/TodoApi/Program.cs:7`** — the composition root registers the concrete Application service directly (`AddScoped<ITodoService, TodoService>()`), while Infrastructure is wired via an `AddTodoInfrastructure()` extension. Asymmetric, and it leaks the concrete `TodoService` choice into the API. *Why it matters:* minor; the API knows an Application implementation detail it shouldn't need to. *Fix:* add an `AddTodoApplication()` extension in the Application layer that registers `ITodoService → TodoService`, and call it from `Program.cs` (mirrors `AddTodoInfrastructure`).

3. **nitpick · removed `tests/TodoApi.Tests/Unit/TodoMappingTests.cs`** — F5 deleted the dedicated mapper tests and replaced them with service-level tests. The Secret/DTO rule and the core entity→DTO projection are still covered (via `TodoServiceTests` Create/Update paths), so coverage is acceptable, but a few direct assertions were dropped (`DueDate` field copy, `TodoItem`/`TodoItemDto` defaulting `Priority` to `Medium` and `Tags` to empty). *Why it matters:* low — these are exercised indirectly. *Fix (optional):* keep a small `TodoMapperTests` for the default/DueDate cases, or fold a `DueDate` assertion into an existing service test.

## Behavior & scope check

- **Behavior preserved.** Integration tests changed only their `using` namespaces (`TodoApi.Data` → `TodoApi.Infrastructure.Data`; `TodoApi.Models` → `TodoApi.Application.Todos` + `TodoApi.Domain.Todos`); no asserts, routes, status codes, or expected shapes were touched. All 30 tests pass. The contract (`201`+`Location`, `204`, `404`, `400` `ValidationProblem`, `X-Total-Count`) is unchanged.
- **Stayed structural.** No new endpoints or functionality; the diff is a clean move + extract-to-service. `overview.md`, `domain-model.md`, `api-contracts.md`, and `INDEX.md` were updated to describe the new structure (an acceptance criterion). No leftover flat namespaces, no template `UnitTest1.cs`, one public type per file.
- **F5 cleaned up after itself.** Inline `Program.cs` logic, duplicated POST/PUT validation, and any over-fetching were genuinely resolved (centralized in `TodoService`, pushed to the repository query) rather than just relocated into new folders.

## Suggested follow-up

Do **finding 1** — drop the duplicated `PropertyGroup`s from the three new `.csproj` files so build settings have a single source of truth in `Directory.Build.props`. Optionally add `AddTodoApplication()` (finding 2) for a symmetric composition root. Everything else is already in good shape.
