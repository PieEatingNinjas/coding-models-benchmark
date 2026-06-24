---
name: spec-authoring
description: How to write specs in spec/ — frontmatter schema, file structure, and the INDEX.md navigation hub. Use in phase 1 before creating or updating specs.
---

# Spec Authoring

> **Related skills:** `orchestration/pipeline-flow`, `development/implementation-workflow`

## Purpose

Specs are the **single source of truth** for sub-agents. They have no context beyond what is in `spec/` — so over-document. One spec per coherent topic.

## Frontmatter schema (required at the top of every spec)

```yaml
---
title: <short title>
category: <domain | data-model | business-logic | api | persistence | io | error-handling | ops>
priority: <1-5>          # 1 = essential context, 5 = edge cases
tags: [<keywords>]
source: [<source files or tickets>]
related_docs: [<other spec files>]
summary: <one line>
---
```

## File structure (guideline)

```
spec/
  INDEX.md                  # navigation hub (see below) — required
  overview.md               # purpose, scope, non-goals
  domain-model.md           # entities, value objects, invariants
  business-logic.md         # rules, calculations, flows
  api-contracts.md          # endpoints, DTOs, status codes
  persistence.md            # EF Core, schema, migrations
  error-handling.md         # exceptions, validation, edge cases
  non-functional.md         # performance, security, observability
```

## INDEX.md — the navigation hub

Required. Contains:
1. **Priority-ordered reading list** (1 → 5).
2. **Docs by category**.
3. **Tags index** (keyword → files).
4. **Open bugs / attention points** as a table.

Sub-agents always start at `INDEX.md` and filter from there by category/tags.

## Rules

- Write concrete and testable; avoid vagueness ("fast", "robust").
- Verify uncertain .NET/Azure APIs via Microsoft Learn MCP before including them.
- One topic per file; cross-reference via `related_docs`.
- Update INDEX.md whenever you add or remove a spec.
