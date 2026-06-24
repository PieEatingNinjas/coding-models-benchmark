---
name: reporting
description: Format for the pipeline's final execution report. Use in the last phase to produce reports/pipeline-execution-report.md.
---

# Reporting

> **Related skills:** `orchestration/pipeline-flow`, `orchestration/sub-agent-dispatch`

## Purpose

One readable report showing what the pipeline did. Write to `reports/pipeline-execution-report.md`.

## Template

```markdown
# Pipeline Execution Report — <date>

## Executive Summary
<2-3 sentences: what was built, status, open risks.>

## Phase log
| Phase | Agent | Status | Duration | Gate met |
|-------|-------|--------|----------|----------|
| 1 Specs | Orchestrator | ✓ | … | ✓ |
| 2 Impl ∥ Test | Developer ∥ Tester | ✓ | … | ✓ |
| 3 Security | Security | ✓ | … | ✓ |
| 4 Remediation | Developer | ✓ | … | ✓ |

## Agent registry
| Agent | Invocations | Files created | Files modified | Issues |
|-------|------------|---------------|----------------|--------|

## Changed files
<list of src/ and tests/ changes>

## Security
<number of findings per severity, what was fixed>

## Open items
<bugs, ambiguities, follow-up work>
```

## Rules

- Factual and concise — no marketing language.
- Mention every failed/retried step explicitly.
- Link to the security report and relevant PRs.
