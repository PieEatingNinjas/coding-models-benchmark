# TodoApi — benchmark baseline

A deliberately simple **.NET 10 Minimal Web API** (todo list, EF Core InMemory) with unit and integration tests. It serves as a fixed starting point for **comparing coding models**: every model implements the same feature from the same baseline. See **`BENCHMARK-PLAN.md`**.

## Structure

```
dotnet-todo-benchmark/
├── TodoApi.sln
├── global.json                 # pins the .NET 10 SDK
├── Directory.Build.props       # shared TFM/nullable/warnings-as-errors for all projects
├── .editorconfig               # shared conventions (same bar for every model)
├── src/TodoApi/                # Minimal Web API
│   ├── Program.cs              #   endpoints under /todoitems
│   ├── Models/                 #   TodoItem, TodoItemDto + TodoMapper
│   └── Data/TodoDb.cs          #   EF Core DbContext (InMemory)
├── tests/TodoApi.Tests/        # xUnit + Bogus + FluentAssertions
│   ├── Unit/                   #   mapping tests
│   ├── Integration/            #   endpoint tests (WebApplicationFactory)
│   └── Support/                #   Bogus fakers + fixed seed (TestSeed.cs)
├── scripts/run-gate.sh         # one objective check per model run (build+test+diff)
├── .github/                    # AI-SDLC structure (agents, skills, spec, workflows)
├── BENCHMARK-PLAN.md           # features + scoring rubric for model comparison
└── RUNBOOK.md                  # copy-paste steps to run the benchmark
```

## Running

```bash
dotnet restore
dotnet build -warnaserror
dotnet test
dotnet run --project src/TodoApi   # API on http://localhost:5179
```

> **Package versions:** the `.csproj` files pin .NET 10 versions (EF Core 10.0.0, Mvc.Testing 10.0.0, xunit 2.9.x, Bogus 35.6.x, FluentAssertions 6.12.x). If an exact patch version isn't available on your feed, let `dotnet restore` pick the nearest one or adjust the version. FluentAssertions is intentionally held at 6.x (7.x is commercially licensed).

## Endpoints (baseline)

| Method | Route | Result |
|--------|-------|--------|
| GET | `/todoitems` | all todos |
| GET | `/todoitems/complete` | completed only |
| GET | `/todoitems/{id}` | one todo (`404` if unknown) |
| POST | `/todoitems` | new todo (`201`) |
| PUT | `/todoitems/{id}` | update (`204` / `404`) |
| DELETE | `/todoitems/{id}` | delete (`204` / `404`) |

Endpoints return `TodoItemDto`; the entity's `Secret` property never leaks to the outside.

## Reproducibility & conventions

- **Deterministic test data:** `tests/.../Support/TestSeed.cs` fixes the Bogus seed and disables test parallelization, so fake data is identical across runs and models — the model is the only variable.
- **Same bar for everyone:** `Directory.Build.props` and `.editorconfig` centralize TFM, nullable, and warnings-as-errors. Projects a model adds later (e.g. in F5) inherit them automatically.

## Next step

Follow **`RUNBOOK.md`**: verify and tag the baseline, give each model a feature plan from `.github/spec/features/`, run `scripts/run-gate.sh`, and score with the rubric in `BENCHMARK-PLAN.md`.
