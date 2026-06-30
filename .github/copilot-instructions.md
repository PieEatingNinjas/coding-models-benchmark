# Copilot Instructions — TodoApi

> Ground rules for EVERY AI agent (Copilot, Claude, …) working in this repo.
> Follow them unless a specific agent or skill instruction explicitly says otherwise.

## Repository

.NET 10 **Minimal Web API** for a todo list, using EF Core (InMemory provider). CRUD endpoints under `/todoitems`. Based on the Microsoft "minimal web API" tutorial. This project is a **fixed baseline for comparing coding models** — see `BENCHMARK-PLAN.md` in the repo root.

- Solution: `TodoApi.sln`
- Production code: `src/TodoApi/`
- Tests: `tests/TodoApi.Tests/`
- Specs / knowledge base: `.github/spec/` (always start at `spec/INDEX.md`)

## Entry Point

**Always start at `OrchestratorAgent`** (`agents/orchestrator.agent.md`). It knows the other agents, plans the work, and dispatches sub-agents.

---

## Ground Rules (KISS — read this first)

These rules exist to avoid "agent loops" (an agent that starts, second-guesses itself, refactors, starts over …).

1. **Just implement it.** The most straightforward implementation that works. Do not over-engineer.
2. **Don't overthink it.** Pick a reasonable approach and execute. The human will troubleshoot afterwards.
3. **Don't be verbose.** Code plus a short status, that's it.
4. **No unsolicited refactors.** Touch only what the task (or feature spec) asks for.
5. **Build & test after EVERY change** (`dotnet build`, `dotnet test`). Do not continue on a red build.
6. **Existing tests stay green.** New behavior means new tests.
7. **No secrets in code, commits, or logs.**

## Agents

Keep agents **small** — small personas with one sharp role.

| Agent | File | Role |
|-------|------|------|
| **OrchestratorAgent** | `agents/orchestrator.agent.md` | Orchestrator. Plans, writes specs, dispatches sub-agents. Writes no code. |
| **DeveloperAgent** | `agents/developer.agent.md` | Implements .NET code from specs. |
| **TesterAgent** | `agents/tester.agent.md` | Writes xUnit/Bogus tests from specs. |
| **SecurityAgent** | `agents/security.agent.md` | Scans for vulnerabilities. |
| **ArchitectureAgent** | `agents/architecture.agent.md` | Reviews Clean Architecture & .NET best practices. Writes a review report; fixes nothing. |
| **DevOpsAgent** | `agents/devops.agent.md` | CI/CD, Docker, Azure. |

## Skills

Reusable, deterministic instructions in `.github/skills/` (orchestration, development, testing, security, build, devops). Each agent states which skills it loads and when.

## MCP Servers

Wired at workspace level in `.vscode/mcp.json` (no auth needed).

| Server | Tools | Used for |
|--------|-------|----------|
| **Microsoft Learn MCP** | `microsoft_docs_search`, `microsoft_docs_fetch`, `microsoft_code_sample_search` | Look up official, current .NET/ASP.NET Core/EF Core/NuGet docs and code samples before using an API. Prefer these over recalling APIs from memory for narrow/specific questions. |

## Key Conventions

- Target framework: **.NET 10**, C# `nullable enable`, `ImplicitUsings enable`, warnings-as-errors.
- Endpoints return **`TodoItemDto`**, never the `TodoItem` entity (the `Secret` property must never leak).
- `async`/`await` end-to-end for I/O; no `.Result`/`.Wait()`.
- Dependency Injection via the built-in container.
- One public type per file; file name == type name.
- Tests: **xUnit + FluentAssertions + Bogus**; unit tests in `tests/.../Unit/`, integration tests (WebApplicationFactory) in `tests/.../Integration/`. Arrange-Act-Assert.
- Test data is deterministic: a fixed Bogus seed and disabled parallelization live in `tests/.../Support/TestSeed.cs` — do not remove or change them.
- Shared build settings live in `Directory.Build.props`; don't duplicate or relax them per project.
- Money/amounts (if ever added): `decimal`. Timestamps: `DateTimeOffset`.

## Build & test

```bash
dotnet restore
dotnet build -warnaserror
dotnet test
```
