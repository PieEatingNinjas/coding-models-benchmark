---
name: sub-agent-dispatch
description: Patterns for dispatching sub-agents with structured instructions — single & parallel dispatch, gating, and result collection. Used by OrchestratorAgent from phase 2.
---

# Sub-Agent Dispatch

> **Related skills:** `orchestration/pipeline-flow`, `orchestration/reporting`

## Dispatch format

Provide on EVERY invocation:
1. **Task** — what to do (one line).
2. **Input** — what to read.
3. **Output** — what to produce.
4. **Skills** — which skills to load.
5. **Done when** — the completion gate.

## Template: single dispatch

```
Invoke <AgentName>:
- Task:    <one line>
- Read:    <files/folders>
- Produce: <expected output files>
- Skills:  <skill paths>
- Done when: <gate>
```

## Template: parallel dispatch

Conditions: no write conflicts, shared input immutable, independent completion.

```
Dispatch in parallel:
  Agent A: <task> → writes to <path A>
  Agent B: <task> → writes to <path B>
Gate: A and B done before the next phase.
```

## Result collection (per agent)

| Field | Description |
|-------|-------------|
| Agent | Which one ran |
| Status | Success / Failed / Partial |
| Files Created | new files |
| Files Modified | changed files |
| Issues | bugs, ambiguities, blockers |

Store this for the final report (`orchestration/reporting`).

## Concrete dispatches

### Phase 2 — DeveloperAgent
```
Task: Implement .NET code from the specs
Read: spec/INDEX.md, spec/*.md
Produce: src/**/*.cs, *.csproj
Skills: development/implementation-workflow, development/dotnet-patterns,
        development/code-checklist, build/build-validation
Done when: dotnet build + dotnet test green, all types implemented
```

### Phase 2 — TesterAgent (parallel)
```
Task: Write tests from the specs
Read: spec/INDEX.md, spec/*.md
Produce: tests/**/*Tests.cs
Skills: testing/unit-testing, testing/integration-testing
Done when: test files created, dotnet test runs, plan documented
```

### Phase 3 — SecurityAgent
```
Task: Scan code for vulnerabilities
Read: src/ (*.cs, *.csproj, appsettings*.json)
Produce: security-reports/security-report.md
Skills: security/code-scanning
Done when: report exists with findings + remediation
```

### Phase 4 — DeveloperAgent (fixes)
```
Task: Fix CRITICAL & HIGH from the security report
Read: security-reports/security-report.md
Produce: fixed files in src/
Skills: development/code-checklist, build/build-validation
Done when: all CRITICAL/HIGH fixed, build + test green
```

## Error handling

| Scenario | Action |
|----------|--------|
| Sub-agent fails | Log, retry 1×, then flag in the report |
| Incomplete output | Note in the report, proceed if non-blocking |
| Gate not met | DO NOT proceed — debug and retry |
