---
name: pipeline-flow
description: Full execution flow for the .NET SDLC pipeline — phases, gates, and parallel execution. Use at the start of every new feature/translation or when planning the order of work.
---

# Pipeline Flow

> **Related skills:** `orchestration/spec-authoring`, `orchestration/sub-agent-dispatch`, `orchestration/reporting`

## Principle

Strictly phased with **gates**: each phase must pass its gate before the next starts. Parallel tasks only if they write to separate paths and read the same immutable input.

## Phases

| # | Phase | Who | Output | Gate |
|---|-------|-----|--------|------|
| 1 | Analysis & Specs | Orchestrator | `spec/*.md`, `spec/INDEX.md` | All specs exist + in INDEX |
| 2 | Implementation + Tests | Developer ∥ Tester | `src/**`, `tests/**` | `dotnet build` + `dotnet test` green |
| 3 | Security review | Security | `security-reports/security-report.md` | Report exists |
| 4 | Remediation | Developer | fixed `src/**` | CRITICAL/HIGH fixed, build green |
| 5 | DevOps (optional) | DevOps | `.github/workflows/**`, IaC | Pipeline validates |
| 6 | Final report | Orchestrator | `reports/pipeline-execution-report.md` | Report complete |

## Parallel execution — conditions

- **No write conflicts:** Developer writes `src/`, Tester writes `tests/`.
- **Shared input is immutable:** specs do not change during phase 2.
- **Independent completion:** each agent's success is independent of the others.

## Gate discipline

- Gate not met → **do not proceed**. Debug or re-dispatch (max 1 retry, then flag in the final report).
- Track per phase: status, changed files, open issues.
