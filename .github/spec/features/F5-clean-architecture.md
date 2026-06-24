---
title: "Feature F5 — Refactor to Clean Architecture"
category: feature-plan
priority: 5
tags: [refactor, clean-architecture, layering, unit-tests]
status: ready
planner_model: "Claude Opus 4.8"
related_docs: [../overview.md, ../domain-model.md, ../api-contracts.md]
summary: Same functionality, split into layers, without changing external behavior.
---

# F5 — Refactor to Clean Architecture (the big test)

> Spec-level plan. Describes WHAT and the approach, not the code.
> This is a **refactor**: external behavior must not change. All existing integration tests stay
> **substantively unchanged** and green — that is the most important guardrail.

## Goal
Restructure the codebase into clear layers with dependencies pointing inward, without changing the observable API.

## Scope — what changes
- **Layer split** into separate projects:
  - **Domain** — entities and domain rules; no dependency on frameworks or persistence.
  - **Application** — use-cases/services and the abstractions they rely on (e.g. a repository interface). Depends only on Domain.
  - **Infrastructure** — concrete implementation of the Application abstractions using EF Core. Depends on Application/Domain.
  - **API** — endpoints + DI wiring; thin. Depends on Application (and wires Infrastructure via DI).
- **Endpoints become thin:** they validate/translate input and call Application services; business logic no longer lives in `Program.cs`.
- **Dependency direction:** everything points inward (API → Application → Domain; Infrastructure → Application/Domain). Domain depends on nothing.

## Hard constraints
- **No change in external behavior:** routes, status codes, request/response shapes stay identical.
- **Existing integration tests stay substantively unchanged** and green. (Only making them reference `Program` may technically remain necessary; do not change asserts or expected outcomes.)
- **The DTO/Secret rule still holds:** the API never leaks internal fields.
- No functional extension — purely structural rearrangement.

## Acceptance criteria
- `dotnet build -warnaserror` green across all projects.
- All existing integration tests pass without substantive changes.
- The Application layer has **new unit tests** with a mocked repository (use-case behavior tested in isolation).
- Dependency direction is correct (Domain has no outgoing project references).
- `overview.md`/`domain-model.md` and INDEX updated with the new structure.

## Tests to add (description)
- **Unit (Application):** each use-case (get/create/update/delete) tested against a mocked repository — happy path and "not found" path.
- **Unit (Application):** the Secret/DTO rule is enforced at the service level.
- Existing integration tests remain the end-to-end guardrail.

## Assessment focus (why F5 differentiates models)
- Can the model stay **in scope** (preserve behavior, not sneak in features)?
- Does it truly respect the **dependency direction**, or do leaking references appear?
- Does the **diff stay manageable** and the wiring correct, with existing tests green?

## Out of scope
New functionality, switching persistence technology, introducing CQRS/MediatR (unless you deliberately want to test that as a separate variant feature).
