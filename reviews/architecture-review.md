# Architecture Review — F5 Clean Architecture

**Scope reviewed:** end state of the playthrough (`baseline...HEAD`), with `HEAD~1...HEAD` as the F5 refactor.
**Build/test:** `dotnet build -warnaserror` green (0 warnings); `dotnet test` green (32 passed: 9 Application unit + 23 existing).

## Verdict
**acceptable-with-smells** — Layering and dependency direction are correct and behavior is preserved. One real architectural problem (in-memory paging) and two smells (duplicated endpoint validation, leftover template test) keep it from "solid".

## What's right
- **Dependency direction is clean.** `Domain` has zero project references; `Application → Domain`; `Infrastructure → Application + Domain`; `API → Application + Infrastructure`. No outward or sideways leaks. (verified in all four `*.csproj`)
- **No EF leak in the API.** `Program.cs` wires only `AddApplication()` + `AddInfrastructure()`; `DbContext`/EF Core live entirely in Infrastructure behind `ITodoRepository`. The InMemory registration sits in `Infrastructure/DependencyInjection.cs`.
- **Secret/DTO rule held.** `TodoItemDto` (in Application) omits `Secret`; entity→DTO mapping is the only path out, and two tests assert the property cannot exist.
- **Behavior preserved.** Integration tests changed by namespace only (`TodoApi.Data`/`TodoApi.Models` → `TodoApi.Infrastructure.Data`/`TodoApi.Domain.*`); no asserts or expected outcomes touched. Routes, status codes, and shapes are identical.

## Findings (most → least severe)

### Problems
1. **`problem` · `src/TodoApi.Infrastructure/Repositories/TodoRepository.cs:13` — in-memory paging / over-fetch.**
   `GetPaginatedAsync` does `await db.Todos.ToListAsync()` and then filters, counts, and `Skip/Take`s in memory (lines 13–24). Even on the no-tag path the entire table is materialized before paging, so the DB never limits the result set. *Why it matters:* this is the easy-to-miss scalability trap the layer split was supposed to fix — it scales O(table) per request regardless of `pageSize`, and tests stay green because the dataset is tiny. *Fix:* page on the `IQueryable` — `db.Todos.Skip((page-1)*pageSize).Take(pageSize)` with a separate `CountAsync()`. The case-insensitive `Tags.Contains` is the only part that legitimately needs client evaluation; isolate it rather than materializing the whole table for every call. (The interface shape is fine — it already returns a paging-aware `(Items, TotalCount)` tuple — so this is implementation-only.)

### Smells
2. **`smell` · `src/TodoApi/Program.cs:48-58` and `:67-77` — validation duplicated across POST and PUT.**
   The three name/due-date checks are copy-pasted verbatim into both endpoints. Thin endpoints that validate/translate are within F5's intent, but the duplication is exactly the F1–F4 shortcut the refactor should have consolidated, not relocated. *Fix:* extract one shared `ValidateTodo(TodoItemDto)` helper (returning the validation result) and call it from both, or move the rule into the Application service and translate its outcome.
3. **`smell` · `tests/TodoApi.Application.Tests/UnitTest1.cs:1-10` — leftover template test added by F5.**
   An empty `Test1()` placeholder was committed alongside the real `TodoServiceTests`. Dead scaffolding; flagged in the same vein as the template `UnitTest1.cs` the rules call out. *Fix:* delete the file.

### Nitpicks
4. **`nitpick` · `src/TodoApi.Application/DTOs/TodoItemDto.cs` — two public types in one file.**
   `TodoItemDto` and `TodoMapper` share `TodoItemDto.cs`, against the repo's one-type-per-file convention. *Fix:* move `TodoMapper` to `TodoMapper.cs`.

## Behavior & scope check
- **Behavior preserved:** yes. Routes, status codes, request/response shapes, `X-Total-Count`, clamping (`page>=1`, `pageSize 1..100`), and case-insensitive tag dedup all match `api-contracts.md`. Integration-test edits are namespace-only.
- **Scope discipline:** held. The diff is purely structural (project split, DI extensions, moved types) plus the spec-mandated Application unit tests. No new endpoints or features snuck in. `INDEX.md`, `domain-model.md`, and `overview.md` were updated for the new structure as required.
- **Tests:** existing integration tests substantively unchanged; new mocked-repository unit tests cover get/create/update/delete happy + not-found paths and the Secret rule, satisfying the F5 acceptance criteria. No weakened asserts observed.

## Suggested follow-up
The one change worth making: **push filtering/paging into the query in `TodoRepository.GetPaginatedAsync`** (finding 1) so the abstraction's paging promise is honored at the data layer. Second, **deduplicate the POST/PUT validation** (finding 2) to finish the cleanup F5 started.
