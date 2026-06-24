# Benchmark plan — comparing coding models on the TodoApi

This document is the starting point for objectively comparing different coding models (or agents). Everyone starts from the **same baseline** (this repo) and implements the **same feature(s)** following a fixed plan. Then you score the results with the same rubric.

## Roles vs. models (read this first)

An "agent" here is a **role** (a prompt/persona in `.github/agents/`), not a model. A **model** is what fills a role. The same model can play any role; what you vary in the benchmark is which model you drop into the role under test.

| Role | What it does | In this benchmark |
|------|--------------|-------------------|
| **Planner / Orchestrator** (Phase 1 = analysis & specs) | Turns a feature into a spec-level plan | **Fixed.** Run once, frozen as the `F*.md` plans (planner = Claude Opus 4.8). |
| **Implementer** (Developer, and Tester) | Writes the code/tests from the plan | **The variable.** This is the model you compare. |
| Security / DevOps | Review & pipeline | Not exercised in the core comparison. |

Note: the **Planner and the Orchestrator are the same role** — Phase 1 of the OrchestratorAgent *is* the planning. We pre-ran that phase and froze its output, so in the core comparison the orchestrator does not run live.

## Two ways to run it

**Mode A — compare implementation (start here).** Hand the implementer model the plan directly (`copilot-instructions.md` + `spec/INDEX.md` + the chosen `F*.md`). No orchestrator in the loop. The planner is fixed; only the implementer model varies. This isolates raw coding ability and is the recommended first phase.

**Mode B — benchmark the whole agent system (later).** Let the model under test drive the full pipeline: it plays the OrchestratorAgent and dispatches the sub-agents. Here the orchestrator *is* the model under test — you're measuring "how well does this model run a team", not just "how well does it code". The orchestrator hardening (operating discipline + project memory) exists for this mode; in Mode A it stays dormant.

> **This project focuses on Mode A first.** Everything below (steps, gate, rubric) describes Mode A. Mode B is a later extension.

## Planning vs. implementation (important)

**Planning** a feature and **implementing** it are kept separate:

- **Planning is done once, by a single fixed planner model.** The plans are already prepared in `.github/spec/features/` (F1–F5), at spec level (WHAT + acceptance criteria + test cases, no code). This keeps the plan a constant so you measure implementation purely.
- **Implementation is done by the models under comparison**, each from the same baseline with the same plan.

> Want a different planner model later? Then regenerate **all** plans with it; don't mix planners within one comparison. Fill in `planner_model` in each plan.

## How it works

1. **Freeze the baseline.** Commit the current repo (green build + tests) as a tag, e.g. `baseline`. Every model starts from here — no building on another model's work.
2. **Pick a feature** from `.github/spec/features/` (F1 → F5, increasing in difficulty).
3. **Give every model the exact same task:** the feature plan (`features/F*.md`) plus the ground rules from `.github/copilot-instructions.md`. Run it via the OrchestratorAgent, or hand over the plan directly — but identical for all models.
4. **Collect each model's result** in a separate branch/worktree: `feat/<feature>/<model>` (e.g. `feat/F1/gpt-x`, `feat/F1/claude-y`).
5. **Score** each result with the rubric below. Fill in the results table.
6. **Reset** to `baseline` for the next feature/model.

> Tip: use `git worktree add ../run-<model> baseline` so every model works in its own clean copy without affecting the others.

## Task template (give this to each model)

```
Repo: TodoApi (.NET 10 Minimal Web API). Read .github/copilot-instructions.md and .github/spec/INDEX.md.
Implement the feature described in: .github/spec/features/F<n>-<name>.md.
Requirements:
- Follow the Ground Rules (KISS, no unsolicited refactors).
- All EXISTING tests stay green.
- Add tests for the new behavior (xUnit + Bogus + FluentAssertions).
- dotnet build -warnaserror and dotnet test must be green.
- Update the relevant spec(s) in .github/spec/ and update spec/INDEX.md.
Deliver: a diff/branch with only the changes for this feature.
```

---

## Features

The full, spec-level plans live in **`.github/spec/features/`** (one file per feature, written in advance by the planner model). Below is just an overview; give the matching file as the task to each model.

| Feature | Plan | Short description |
|---------|------|-------------------|
| F1 | `features/F1-priority.md` | Priority (`Low/Medium/High`) + filter endpoint (warm-up) |
| F2 | `features/F2-due-dates.md` | Optional due date, past-date validation (ProblemDetails), overdue overview |
| F3 | `features/F3-tags.md` | Tags per todo + case-insensitive filtering |
| F4 | `features/F4-validation-pagination.md` | Name validation, ProblemDetails, pagination with total |
| F5 | `features/F5-clean-architecture.md` | Refactor into layers, preserve behavior, unit tests on the Application layer (the big test) |

> Start with F1 as a warm-up; F5 is the big test (refactoring within boundaries — often where models differ the most).

---

## Scoring rubric (per model, per feature)

Score each criterion 0–5. Sum to a total out of 30.

| # | Criterion | What you assess | 0 | 5 |
|---|-----------|-----------------|---|---|
| 1 | **Correctness** | Build green, all tests green, feature works | broken | all green, behavior correct |
| 2 | **Spec adherence** | Does exactly what the plan asks (endpoints, status codes, rules) | deviates | fully compliant |
| 3 | **Test quality** | New tests cover the behavior, meaningful asserts, uses Bogus | none/weak | thorough + meaningful |
| 4 | **Conventions** | Follows `copilot-instructions.md` (DTO, async, nullable, one type/file) | ignores | fully |
| 5 | **Scope discipline** | No unsolicited refactors; diff stays on the feature | sprawls | tight and minimal |
| 6 | **Maintainability** | Readable, no dead code, specs updated | messy | clean + documented |

**Practical metrics to record alongside the score:**
- Number of attempts/iterations to green.
- Diff size (lines changed/added) — smaller is often better for the same result.
- Time and/or token usage.
- Did the model stay within the requested scope or wander off?

## Results table (copy per feature)

```
Feature: F_
| Model | Correct | Spec | Tests | Conventions | Scope | Maint. | Total/30 | Iterations | Diff (lines) | Time | Notes |
|-------|---------|------|-------|-------------|-------|--------|----------|------------|--------------|------|-------|
|       |         |      |       |             |       |        |          |            |              |      |       |
```

## Verify objectively (not by eye)

For each model result, run the exact same gate:

```bash
dotnet build -warnaserror
dotnet test
git diff --stat baseline   # diff size and scope
```

Only once build + tests are green does the result count. That way you compare apples to apples.
