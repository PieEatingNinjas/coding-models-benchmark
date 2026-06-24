# Orchestrator memory — lessons learned

Durable, planner/orchestrator-side notes that make each pipeline run smarter than the last.

> **Scope:** maintained by the OrchestratorAgent only. Do **not** give this file to implementer
> models — the benchmark's implementer input is just `.github/copilot-instructions.md`,
> `.github/spec/INDEX.md`, and the chosen feature plan. Keeping memory out of that input
> preserves the controlled variable.

## How to use
- **Phase 0 (run start):** read this file; apply anything relevant.
- **Phase 6 (run end):** append a short, dated entry below.

## Entry format
```
### YYYY-MM-DD — <feature / context>
- What happened: <gate failure, surprise, ambiguity>
- Fix / decision: <what resolved it>
- Watch next time: <pitfall to avoid>
```

## Log

### 2026-06-24 — Baseline established
- What happened: Initial TodoApi baseline created (.NET 10 Minimal Web API + xUnit/Bogus tests).
- Fix / decision: Endpoints return `TodoItemDto`; `Secret` never leaks. Fixed Bogus seed + disabled test parallelization for determinism. Shared build settings centralized in `Directory.Build.props`.
- Watch next time: Verify package versions resolve on the target feed (`dotnet restore`); keep the test seed and shared build props untouched when implementing features.
