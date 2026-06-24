---
title: "Feature F1 — Priority"
category: feature-plan
priority: 1
tags: [priority, filter, enum]
status: ready
planner_model: "Claude Opus 4.8"
related_docs: [../domain-model.md, ../api-contracts.md]
summary: Todos get a priority and you can filter on it.
---

# F1 — Priority

> Spec-level plan. Describes WHAT and the approach, not the code. The implementer designs the code itself.
> Constraint: all existing tests stay unchanged and green; new behavior gets new tests.

## Goal
A todo can have a priority (`Low`, `Medium`, `High`). Users can fetch todos by priority.

## Scope — what changes
- **Domain model:** add a priority concept to the todo entity, with a finite set of values (`Low`/`Medium`/`High`). Default value `Medium`.
- **Contract (DTO):** priority becomes part of the public contract and is mapped.
- **Write paths:** POST and PUT carry priority; if absent from the body, the default applies.
- **New endpoint:** fetch by priority, as a sub-resource of `/todoitems`.

## Behavior / rules
- A new todo without a specified priority gets `Medium`.
- The filter endpoint returns only todos with exactly the requested priority.
- An unknown/invalid priority value in the filter yields a client error (`400`), not an empty `200`.
- The serialization form of priority (text vs number) is chosen by the implementer and documented in `api-contracts.md`; be consistent between reading and writing.

## Endpoints (to document in api-contracts.md)
- `GET /todoitems/by-priority/{priority}` → `200` with the filtered list; `400` for an invalid value.
- Existing CRUD endpoints: behavior unchanged, except that the response/POST/PUT now include priority.

## Acceptance criteria
- `dotnet build -warnaserror` and `dotnet test` green.
- Priority is in every DTO response.
- Default `Medium` is applied when absent.
- Filter endpoint correct, including `400` on an invalid value.
- `api-contracts.md`, `domain-model.md`, and `spec/INDEX.md` updated.

## Tests to add (description, not the code)
- **Unit (mapping):** mapping preserves priority; default `Medium` when not set.
- **Integration:** POST without priority → response has `Medium`.
- **Integration:** POST with `High` → found via `by-priority/High`, not via `by-priority/Low`.
- **Integration:** `by-priority/<invalid>` → `400`.
- Use Bogus to generate todos with random priorities where useful.

## Out of scope
Sorting by priority, priority-based business rules, UI. Storage, contract, and filter only.
